using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.AI
{
    public class AIGeneratedDescriptionDto
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public string GeneratedText { get; set; } = string.Empty;
        public string ModelVersion { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class GenerateDescriptionDto
    {
        [Required]
        public int AnimalId { get; set; }
    }

    public class ReviewDescriptionDto
    {
        // Staff can tweak the wording before approving
        [Required]
        public string FinalText { get; set; } = string.Empty;

        [Required]
        public bool Approve { get; set; }
    }
}
