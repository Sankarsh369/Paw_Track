using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.AiDescription
{
    public class ApproveAiAnimalDescriptionDto
    {
        public bool IsApproved { get; set; } = true;

        [MaxLength(500)]
        public string? ReviewNotes { get; set; }
    }
}
