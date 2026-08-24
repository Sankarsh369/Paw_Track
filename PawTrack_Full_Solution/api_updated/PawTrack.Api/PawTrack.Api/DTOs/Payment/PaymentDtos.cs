using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.Payment
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int? AdoptionId { get; set; }
        public int? AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public int? DonorUserId { get; set; }
        public string? DonorName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public string TransactionId { get; set; } = string.Empty;

        // Sent to the frontend so it can launch the real Razorpay Checkout modal.
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayKeyId { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
    }

    // Kicks off a checkout — PawTrack never collects card/bank details directly,
    // this only records intent; a gateway webhook confirms it (see ConfirmPaymentDto).
    public class CreateAdoptionFeePaymentDto
    {
        [Required]
        public int AdoptionId { get; set; }

        [Required, Range(0.01, 100000)]
        public decimal Amount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "USD";
    }

    public class CreateDonationDto
    {
        // Optional — donate toward a specific animal, or omit for the general fund
        public int? AnimalId { get; set; }

        [Required, Range(1, 1000000)]
        public decimal Amount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        // True for a guest/anonymous donor even while logged out
        public bool Anonymous { get; set; } = false;
    }

    // Simulates the payment gateway webhook confirming a transaction server-side —
    // per the requirement that status is confirmed via webhook, not just client redirect.
    public class ConfirmPaymentDto
    {
        [Required, MaxLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty; // Completed / Failed / Refunded

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }
    }

    // What the frontend sends back after Razorpay Checkout succeeds client-side —
    // the signature is verified server-side against KeySecret before trusting it.
    public class VerifyPaymentDto
    {
        [Required]
        public string RazorpayOrderId { get; set; } = string.Empty;

        [Required]
        public string RazorpayPaymentId { get; set; } = string.Empty;

        [Required]
        public string RazorpaySignature { get; set; } = string.Empty;
    }
}
