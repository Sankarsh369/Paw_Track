using PawTrack.Core.Enums;

namespace PawTrack.Core.Entities;

public class FollowUp
{
    public int Id { get; set; }
    public int AdoptionId { get; set; }
    public int ConductedById { get; set; }
    public DateTime FollowUpDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public FollowUpStatus Status { get; set; } = FollowUpStatus.Scheduled;

    // Navigation properties
    public Adoption? Adoption { get; set; }
    public User? ConductedBy { get; set; }
}
