using PawTrack.Api.DTOs.Payment;

namespace PawTrack.Api.Services
{
    public interface IPaymentService
    {
        Task<PaymentDto> InitiateAdoptionFeeAsync(int adopterId, CreateAdoptionFeePaymentDto dto);
        Task<PaymentDto> InitiateDonationAsync(int? donorUserId, CreateDonationDto dto);
        Task<PaymentDto?> VerifyAsync(VerifyPaymentDto dto);
        Task<PaymentDto?> ConfirmAsync(ConfirmPaymentDto dto);
        Task<List<PaymentDto>> GetForUserAsync(int userId);
        Task<List<PaymentDto>> GetAllAsync(string? type, string? status);
    }
}
