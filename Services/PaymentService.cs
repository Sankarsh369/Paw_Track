using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.Payment;
using PawTrack.Api.Models;
using System.Security.Cryptography;
using System.Text;
using RazorpayApi = Razorpay.Api;

namespace PawTrack.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PawTrackDbContext _context;
        private readonly IConfiguration _config;

        public PaymentService(PawTrackDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private RazorpayApi.RazorpayClient GetClient()
        {
            var keyId = _config["Razorpay:KeyId"];
            var keySecret = _config["Razorpay:KeySecret"];
            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
                throw new InvalidOperationException("Razorpay:KeyId / Razorpay:KeySecret are not configured. Set them via user-secrets.");
            return new RazorpayApi.RazorpayClient(keyId, keySecret);
        }

        // Creates a real Razorpay Order for the given amount and returns its id.
        // Amount must be converted to the smallest currency subunit (paise for INR).
        private string CreateRazorpayOrder(decimal amount, string currency, string receipt)
        {
            var client = GetClient();
            var options = new Dictionary<string, object>
            {
                { "amount", (int)(amount * 100) },
                { "currency", currency },
                { "receipt", receipt },
                { "payment_capture", 1 }
            };
            RazorpayApi.Order order = client.Order.Create(options);
            return order["id"].ToString()!;
        }

        // Razorpay's own documented algorithm (not relying on the SDK's Utils helper,
        // whose exact overload varies by package version):
        // generated_signature = HMAC_SHA256(order_id + "|" + payment_id, key_secret)
        // Compare hex digest to the razorpay_signature the client sends back.
        private bool VerifySignature(string orderId, string paymentId, string signature, string keySecret)
        {
            var payload = $"{orderId}|{paymentId}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var generatedSignature = Convert.ToHexString(hash).ToLowerInvariant();
            return generatedSignature == signature.ToLowerInvariant();
        }

        public async Task<PaymentDto> InitiateAdoptionFeeAsync(int adopterId, CreateAdoptionFeePaymentDto dto)
        {
            var adoption = await _context.Adoptions.FirstOrDefaultAsync(a => a.Id == dto.AdoptionId && a.AdopterId == adopterId);
            if (adoption is null)
                throw new KeyNotFoundException("Adoption not found for this adopter.");

            var payment = new PawTrack.Api.Models.Payment
            {
                Type = PaymentType.AdoptionFee,
                AdoptionId = dto.AdoptionId,
                DonorUserId = adopterId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                TransactionId = $"PT-FEE-{Guid.NewGuid():N}"[..20],
                Status = PaymentStatus.Pending
            };

            payment.RazorpayOrderId = CreateRazorpayOrder(dto.Amount, dto.Currency, payment.TransactionId);

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return ToDto(payment);
        }

        public async Task<PaymentDto> InitiateDonationAsync(int? donorUserId, CreateDonationDto dto)
        {
            if (dto.AnimalId.HasValue)
            {
                var animalExists = await _context.Animals.AnyAsync(a => a.Id == dto.AnimalId.Value);
                if (!animalExists)
                    throw new KeyNotFoundException($"Animal with Id {dto.AnimalId} was not found.");
            }

            var payment = new PawTrack.Api.Models.Payment
            {
                Type = PaymentType.Donation,
                AnimalId = dto.AnimalId,
                DonorUserId = dto.Anonymous ? null : donorUserId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                TransactionId = $"PT-DON-{Guid.NewGuid():N}"[..20],
                Status = PaymentStatus.Pending
            };

            payment.RazorpayOrderId = CreateRazorpayOrder(dto.Amount, dto.Currency, payment.TransactionId);

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return ToDto(payment);
        }

        // Real confirmation path: verifies the cryptographic signature Razorpay returns
        // after a successful Checkout. Only marks Completed if it actually matches —
        // this is what stops someone from just POSTing a fake "success" from the browser.
        public async Task<PaymentDto?> VerifyAsync(VerifyPaymentDto dto)
        {
            var payment = await _context.Payments
                .Include(p => p.Animal)
                .Include(p => p.Donor)
                .FirstOrDefaultAsync(p => p.RazorpayOrderId == dto.RazorpayOrderId);

            if (payment is null) return null;

            var keySecret = _config["Razorpay:KeySecret"]
                ?? throw new InvalidOperationException("Razorpay:KeySecret is not configured.");

            bool isValid = VerifySignature(dto.RazorpayOrderId, dto.RazorpayPaymentId, dto.RazorpaySignature, keySecret);

            if (!isValid)
            {
                payment.Status = PaymentStatus.Failed;
                await _context.SaveChangesAsync();
                return ToDto(payment);
            }

            payment.Status = PaymentStatus.Completed;
            payment.TransactionId = dto.RazorpayPaymentId; // overwrite with the real gateway payment id
            payment.PaymentMethod = "Razorpay";
            payment.PaymentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ToDto(payment);
        }

        // Kept for manual/admin override during testing — real payments go through VerifyAsync.
        public async Task<PaymentDto?> ConfirmAsync(ConfirmPaymentDto dto)
        {
            var payment = await _context.Payments
                .Include(p => p.Animal)
                .Include(p => p.Donor)
                .FirstOrDefaultAsync(p => p.TransactionId == dto.TransactionId);

            if (payment is null) return null;

            payment.Status = Enum.Parse<PaymentStatus>(dto.Status, ignoreCase: true);
            payment.PaymentMethod = dto.PaymentMethod ?? payment.PaymentMethod;
            if (payment.Status == PaymentStatus.Completed)
                payment.PaymentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ToDto(payment);
        }

        public async Task<List<PaymentDto>> GetForUserAsync(int userId)
        {
            return await _context.Payments
                .Include(p => p.Animal)
                .Include(p => p.Donor)
                .Where(p => p.DonorUserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        public async Task<List<PaymentDto>> GetAllAsync(string? type, string? status)
        {
            var query = _context.Payments
                .Include(p => p.Animal)
                .Include(p => p.Donor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<PaymentType>(type, true, out var parsedType))
                query = query.Where(p => p.Type == parsedType);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
                query = query.Where(p => p.Status == parsedStatus);

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        private PaymentDto ToDto(PawTrack.Api.Models.Payment p) => new()
        {
            Id = p.Id,
            Type = p.Type.ToString(),
            AdoptionId = p.AdoptionId,
            AnimalId = p.AnimalId,
            AnimalName = p.Animal?.Name,
            DonorUserId = p.DonorUserId,
            DonorName = p.Donor?.Name,
            Amount = p.Amount,
            Currency = p.Currency,
            PaymentMethod = p.PaymentMethod,
            TransactionId = p.TransactionId,
            RazorpayOrderId = p.RazorpayOrderId,
            RazorpayKeyId = p.Status == PaymentStatus.Pending ? _config["Razorpay:KeyId"] : null,
            Status = p.Status.ToString(),
            PaymentDate = p.PaymentDate
        };
    }
}