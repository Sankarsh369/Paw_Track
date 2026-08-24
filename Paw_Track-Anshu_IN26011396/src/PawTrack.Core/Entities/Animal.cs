using PawTrack.Core.Enums;

namespace PawTrack.Core.Entities;

public class Animal
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = "Male";
    public DateTime RescueDate { get; set; }
    public string RescueLocation { get; set; } = string.Empty;
    public AnimalStatus Status { get; set; } = AnimalStatus.Available;
    public int BranchId { get; set; }
    public int CategoryId { get; set; }
    public string? MicrochipNumber { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    // Concurrency token / RowVersion for optimistic concurrency
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Branch? Branch { get; set; }
    public Category? Category { get; set; }
    public ICollection<VisitBooking> VisitBookings { get; set; } = new List<VisitBooking>();
    public ICollection<AdoptionApplication> AdoptionApplications { get; set; } = new List<AdoptionApplication>();
    public Adoption? Adoption { get; set; }
}
