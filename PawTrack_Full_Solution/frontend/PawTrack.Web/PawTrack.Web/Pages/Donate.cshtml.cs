using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PawTrack.Web.Pages
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class DonateModel : PageModel
    {
        private readonly ApiClient _api;

        public DonateModel(ApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public AnimalModel? TargetAnimal { get; set; }
        public string? ErrorMessage { get; set; }
        public bool Confirmed { get; set; }
        public string? TransactionId { get; set; }

        // Set once a Razorpay Order has been created — tells the view to open Checkout.
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayKeyId { get; set; }

        public class InputModel
        {
            public int? AnimalId { get; set; }

            [Required, Range(1, 1000000, ErrorMessage = "Enter an amount of at least $1.")]
            public decimal Amount { get; set; } = 25;

            public bool Anonymous { get; set; }
        }

        public async Task OnGetAsync(int? animalId, bool? success, string? txn)
        {
            if (success == true)
            {
                Confirmed = true;
                TransactionId = txn;
                return;
            }

            Input.AnimalId = animalId;
            if (animalId.HasValue)
            {
                var (_, animal, _) = await _api.GetAsync<AnimalModel>($"api/animal/{animalId}");
                TargetAnimal = animal;
            }
        }

        // Step 1: create the donation record + a real Razorpay Order.
        // Does NOT mark it Completed — that only happens after Checkout succeeds
        // and the signature is verified (see OnPostVerifyAsync below).
        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.AnimalId.HasValue)
            {
                var (_, animal, _) = await _api.GetAsync<AnimalModel>($"api/animal/{Input.AnimalId}");
                TargetAnimal = animal;
            }

            if (!ModelState.IsValid) return Page();

            var (success, payment, error) = await _api.PostAsync<PaymentModel>("api/payment/donation", new
            {
                animalId = Input.AnimalId,
                amount = Input.Amount,
                currency = "INR",
                anonymous = Input.Anonymous
            });

            if (!success || payment is null || string.IsNullOrEmpty(payment.RazorpayOrderId))
            {
                ErrorMessage = error ?? "Couldn't start that donation right now.";
                return Page();
            }

            RazorpayOrderId = payment.RazorpayOrderId;
            RazorpayKeyId = payment.RazorpayKeyId;
            return Page();
        }

        // Step 2: called via JS fetch() right after Razorpay Checkout succeeds client-side.
        // Forwards the three values to the API, which verifies the signature server-side
        // before trusting it. IgnoreAntiforgeryToken because this is called like an API
        // endpoint from JS, not a normal form post.
        public async Task<IActionResult> OnPostVerifyAsync()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var payload = JsonSerializer.Deserialize<VerifyPayload>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (payload is null)
                return new JsonResult(new { success = false, message = "Bad request." });

            var (success, payment, error) = await _api.PostAsync<PaymentModel>("api/payment/verify", new
            {
                razorpayOrderId = payload.RazorpayOrderId,
                razorpayPaymentId = payload.RazorpayPaymentId,
                razorpaySignature = payload.RazorpaySignature
            });

            if (!success || payment is null || payment.Status != "Completed")
                return new JsonResult(new { success = false, message = error ?? "Payment could not be verified." });

            return new JsonResult(new { success = true, transactionId = payment.TransactionId });
        }

        public class VerifyPayload
        {
            public string RazorpayOrderId { get; set; } = string.Empty;
            public string RazorpayPaymentId { get; set; } = string.Empty;
            public string RazorpaySignature { get; set; } = string.Empty;
        }
    }
}
