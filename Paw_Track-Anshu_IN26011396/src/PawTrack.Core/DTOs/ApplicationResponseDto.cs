using PawTrack.Core.Enums;

namespace PawTrack.Core.DTOs;

public class ApplicationResponseDto
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public string AnimalSpecies { get; set; } = string.Empty;
    public string AnimalBreed { get; set; } = string.Empty;
    public AnimalStatus AnimalStatus { get; set; }
    public int AdopterId { get; set; }
    public string AdopterName { get; set; } = string.Empty;
    public string AdopterEmail { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int? VisitBookingId { get; set; }
    public DateTime ApplicationDate { get; set; }
    public ApplicationStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public LinkedVisitDto? LinkedVisit { get; set; }
}

public class LinkedVisitDto
{
    public int Id { get; set; }
    public DateTime BookingDate { get; set; }
    public VisitBookingStatus Status { get; set; }
    public DateTime? SlotDate { get; set; }
    public string SlotTime { get; set; } = string.Empty;
}
