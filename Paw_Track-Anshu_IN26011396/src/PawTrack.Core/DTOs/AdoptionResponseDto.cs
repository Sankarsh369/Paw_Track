namespace PawTrack.Core.DTOs;

public class AdoptionResponseDto
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public int AnimalId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public string AnimalSpecies { get; set; } = string.Empty;
    public string AnimalBreed { get; set; } = string.Empty;
    public string? AnimalImageUrl { get; set; }
    public int AdopterId { get; set; }
    public string AdopterName { get; set; } = string.Empty;
    public string AdopterEmail { get; set; } = string.Empty;
    public int ApprovedById { get; set; }
    public string ApprovedByName { get; set; } = string.Empty;
    public DateTime AdoptionDate { get; set; }
    public string? Notes { get; set; }
    public int? BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int FollowUpCount { get; set; }
    public List<FollowUpResponseDto> FollowUpHistory { get; set; } = new List<FollowUpResponseDto>();
}
