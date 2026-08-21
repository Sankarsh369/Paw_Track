using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawTrack.Api.Models
{
    public enum SlotStatus
    {
        Available,
        FullyBooked,
        Cancelled
    }

    // Owned by: Tanishq-DasIN26010306 module
    // Represents a time slot at a branch during which adopters can visit animals.
    public class VisitSlot
    {
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }

        [Required]
        public DateTime SlotStart { get; set; }

        [Required]
        public DateTime SlotEnd { get; set; }

        /// <summary>Maximum number of simultaneous bookings allowed for this slot.</summary>
        [Required]
        [Range(1, 100)]
        public int Capacity { get; set; } = 1;

        [Required]
        public SlotStatus Status { get; set; } = SlotStatus.Available;

        // Navigation: all bookings that reference this slot
        public ICollection<VisitBooking>? Bookings { get; set; }
    }
}
