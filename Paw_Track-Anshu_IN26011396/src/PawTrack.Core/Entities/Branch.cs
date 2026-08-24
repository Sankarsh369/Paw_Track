namespace PawTrack.Core.Entities;

public class Branch
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Animal> Animals { get; set; } = new List<Animal>();
    public ICollection<VisitSlot> VisitSlots { get; set; } = new List<VisitSlot>();
}
