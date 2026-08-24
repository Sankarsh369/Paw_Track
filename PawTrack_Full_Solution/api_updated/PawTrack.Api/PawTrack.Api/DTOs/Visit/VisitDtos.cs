using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.Visit
{
    public class VisitSlotDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime SlotDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Capacity { get; set; }
        public int BookedCount { get; set; }
        public bool IsFull => BookedCount >= Capacity;
    }

    public class CreateVisitSlotDto
    {
        [Required]
        public int BranchId { get; set; }

        [Required]
        public DateTime SlotDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required, Range(1, 100)]
        public int Capacity { get; set; }
    }

    public class VisitBookingDto
    {
        public int Id { get; set; }
        public int VisitSlotId { get; set; }
        public DateTime SlotDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public int AdopterId { get; set; }
        public string? AdopterName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime BookedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateVisitBookingDto
    {
        [Required]
        public int VisitSlotId { get; set; }

        [Required]
        public int AnimalId { get; set; }
    }

    public class UpdateVisitBookingStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Booked/CheckedIn/Completed/Cancelled/NoShow

        [MaxLength(300)]
        public string? Notes { get; set; }
    }
}
