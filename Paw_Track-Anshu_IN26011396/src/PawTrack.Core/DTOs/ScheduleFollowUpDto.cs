using System.ComponentModel.DataAnnotations;

namespace PawTrack.Core.DTOs;

public class ScheduleFollowUpDto
{
    [Required]
    public int AdoptionId { get; set; }

    [Required(ErrorMessage = "Follow-up date is required.")]
    public DateTime FollowUpDate { get; set; }

    [Required(ErrorMessage = "Follow-up notes are required.")]
    [MinLength(1, ErrorMessage = "Follow-up notes cannot be empty.")]
    public string Notes { get; set; } = string.Empty;
}

public class CompleteFollowUpDto
{
    public string? NotesUpdate { get; set; }
}
