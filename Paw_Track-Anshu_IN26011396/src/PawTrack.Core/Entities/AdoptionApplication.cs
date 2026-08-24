using PawTrack.Core.Enums;

namespace PawTrack.Core.Entities;

public class AdoptionApplication
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public int AdopterId { get; set; }
    public int? VisitBookingId { get; set; }
    public DateTime ApplicationDate { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }

    // Concurrency token / RowVersion
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Animal? Animal { get; set; }
    public User? Adopter { get; set; }
    public VisitBooking? VisitBooking { get; set; }
    public Adoption? Adoption { get; set; }
}
