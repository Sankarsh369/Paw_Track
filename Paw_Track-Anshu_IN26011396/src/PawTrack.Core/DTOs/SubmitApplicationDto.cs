using System.ComponentModel.DataAnnotations;

namespace PawTrack.Core.DTOs;

public class SubmitApplicationDto
{
    [Required]
    public int AnimalId { get; set; }

    public int? VisitBookingId { get; set; }

    public string? Notes { get; set; }
}
