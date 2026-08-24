using PawTrack.Core.Enums;

namespace PawTrack.Core.DTOs;

public class FollowUpResponseDto
{
    public int Id { get; set; }
    public int AdoptionId { get; set; }
    public int ConductedById { get; set; }
    public string ConductedByName { get; set; } = string.Empty;
    public string ConductedByRole { get; set; } = string.Empty;
    public DateTime FollowUpDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public FollowUpStatus Status { get; set; }
}
