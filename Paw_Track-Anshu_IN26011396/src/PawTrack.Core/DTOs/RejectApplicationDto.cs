using System.ComponentModel.DataAnnotations;

namespace PawTrack.Core.DTOs;

public class RejectApplicationDto
{
    [Required(ErrorMessage = "Rejection reason is required.")]
    [MinLength(1, ErrorMessage = "Rejection reason cannot be empty.")]
    public string RejectionReason { get; set; } = string.Empty;
}
