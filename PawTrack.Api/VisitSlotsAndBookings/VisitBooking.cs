using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawTrack.Api.Models
{
    public enum BookingStatus
    {
        Confirmed,
        Cancelled,
        Completed,
        NoShow
    }

    // Owned by: Tanishq-DasIN26010306 module
    // An adopter's booking for a specific VisitSlot.
    public class VisitBooking
    {
        public int Id { get; set; }

        [Required]
        public int VisitSlotId { get; set; }

        [ForeignKey(nameof(VisitSlotId))]
        public VisitSlot? VisitSlot { get; set; }

        /// <summary>The adopter who made the booking (User with Role = Adopter).</summary>
        [Required]
        public int AdopterId { get; set; }

        [ForeignKey(nameof(AdopterId))]
        public User? Adopter { get; set; }

        /// <summary>The specific animal the adopter wishes to visit (optional — can be null for open visits).</summary>
        public int? AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
