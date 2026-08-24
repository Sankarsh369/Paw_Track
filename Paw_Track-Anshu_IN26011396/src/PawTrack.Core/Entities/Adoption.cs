namespace PawTrack.Core.Entities;

public class Adoption
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public int AnimalId { get; set; }
    public int AdopterId { get; set; }
    public int ApprovedById { get; set; }
    public DateTime AdoptionDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public AdoptionApplication? Application { get; set; }
    public Animal? Animal { get; set; }
    public User? Adopter { get; set; }
    public User? ApprovedBy { get; set; }
    public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
}
