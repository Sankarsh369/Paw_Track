using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.Behavior
{
    public class BehaviorRecordDto
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public int AssessedById { get; set; }
        public string? AssessedByName { get; set; }
        public string Temperament { get; set; } = string.Empty;
        public bool CompatibilityWithKids { get; set; }
        public bool CompatibilityWithPets { get; set; }
        public string? SpecialRequirements { get; set; }
        public DateTime AssessedAt { get; set; }
    }

    public class CreateBehaviorRecordDto
    {
        [Required]
        public int AnimalId { get; set; }

        [Required, MaxLength(200)]
        public string Temperament { get; set; } = string.Empty;

        [Required]
        public bool CompatibilityWithKids { get; set; }

        [Required]
        public bool CompatibilityWithPets { get; set; }

        [MaxLength(300)]
        public string? SpecialRequirements { get; set; }
    }
}
