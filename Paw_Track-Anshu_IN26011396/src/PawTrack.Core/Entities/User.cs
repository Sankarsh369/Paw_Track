using PawTrack.Core.Enums;

namespace PawTrack.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int? BranchId { get; set; } // Null for OrgAdmin

    // Navigation properties
    public Branch? Branch { get; set; }
    public ICollection<VisitBooking> VisitBookings { get; set; } = new List<VisitBooking>();
    public ICollection<AdoptionApplication> AdoptionApplications { get; set; } = new List<AdoptionApplication>();
    public ICollection<Adoption> AdoptionsAsAdopter { get; set; } = new List<Adoption>();
    public ICollection<Adoption> AdoptionsApproved { get; set; } = new List<Adoption>();
    public ICollection<FollowUp> FollowUpsConducted { get; set; } = new List<FollowUp>();
}
