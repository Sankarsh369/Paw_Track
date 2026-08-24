using System.ComponentModel.DataAnnotations;
using PawTrack.Core.Enums;

namespace PawTrack.Core.DTOs;

public class BookVisitDto
{
    [Required]
    public int VisitSlotId { get; set; }

    [Required]
    public int AnimalId { get; set; }
}

public class VisitBookingResponseDto
{
    public int Id { get; set; }
    public int VisitSlotId { get; set; }
    public int AnimalId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public string AnimalSpecies { get; set; } = string.Empty;
    public int AdopterId { get; set; }
    public string AdopterName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime? SlotDate { get; set; }
    public string SlotTime { get; set; } = string.Empty;
    public VisitBookingStatus Status { get; set; }
    public string? StaffNotes { get; set; }
}

public class VisitSlotResponseDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
    public int BookedCount { get; set; }
    public int AvailableSpots => Capacity - BookedCount;
}
