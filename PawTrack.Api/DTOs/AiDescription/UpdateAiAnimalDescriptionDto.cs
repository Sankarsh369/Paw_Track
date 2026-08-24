using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.AiDescription
{
    public class UpdateAiAnimalDescriptionDto
    {
        [Required]
        public string GeneratedText { get; set; } = string.Empty;
    }
}
