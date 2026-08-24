using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawTrack.Api.Models
{
    public enum PaymentType
    {
        AdoptionFee,
        Donation
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    // Payment & Donation module — one ledger, typed. No card/bank details ever stored here.
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public PaymentType Type { get; set; }

        // Set only when Type = AdoptionFee
        public int? AdoptionId { get; set; }

        [ForeignKey(nameof(AdoptionId))]
        public Adoption? Adoption { get; set; }

        // Optional — lets a donor target a specific animal's care
        public int? AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        // Null allowed for one-off/guest donations
        public int? DonorUserId { get; set; }

        [ForeignKey(nameof(DonorUserId))]
        public User? Donor { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required, MaxLength(3)]
        public string Currency { get; set; } = "USD";

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [Required, MaxLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        // The Razorpay Order ID created when the payment is initiated —
        // needed to match the client's success callback back to this row.
        [MaxLength(100)]
        public string? RazorpayOrderId { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime? PaymentDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
