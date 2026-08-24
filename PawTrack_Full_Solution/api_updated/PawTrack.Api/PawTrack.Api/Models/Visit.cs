using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawTrack.Api.Models
{
    public enum VisitBookingStatus
    {
        Booked,
        CheckedIn,
        Completed,
        Cancelled,
        NoShow
    }

    // Visit & Scheduling module
    public class VisitSlot
    {
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }

        [Required]
        public DateTime SlotDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required, Range(1, 100)]
        public int Capacity { get; set; }

        public int BookedCount { get; set; } = 0;

        public ICollection<VisitBooking>? Bookings { get; set; }
    }

    public class VisitBooking
    {
        public int Id { get; set; }

        [Required]
        public int VisitSlotId { get; set; }

        [ForeignKey(nameof(VisitSlotId))]
        public VisitSlot? VisitSlot { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        [Required]
        public int AdopterId { get; set; }

        [ForeignKey(nameof(AdopterId))]
        public User? Adopter { get; set; }

        [Required]
        public VisitBookingStatus Status { get; set; } = VisitBookingStatus.Booked;

        public DateTime BookedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(300)]
        public string? Notes { get; set; }
    }
}
