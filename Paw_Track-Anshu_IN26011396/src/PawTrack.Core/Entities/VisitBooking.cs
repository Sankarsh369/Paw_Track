using PawTrack.Core.Enums;

namespace PawTrack.Core.Entities;

public class VisitBooking
{
    public int Id { get; set; }
    public int VisitSlotId { get; set; }
    public int AnimalId { get; set; }
    public int AdopterId { get; set; }
    public DateTime BookingDate { get; set; }
    public VisitBookingStatus Status { get; set; } = VisitBookingStatus.Booked;
    public string? StaffNotes { get; set; }

    // Navigation properties
    public VisitSlot? VisitSlot { get; set; }
    public Animal? Animal { get; set; }
    public User? Adopter { get; set; }
    public ICollection<AdoptionApplication> AdoptionApplications { get; set; } = new List<AdoptionApplication>();
}
