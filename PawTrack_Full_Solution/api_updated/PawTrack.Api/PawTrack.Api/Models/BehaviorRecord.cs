using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawTrack.Api.Models
{
    // Behavior Management module — temperament & compatibility assessment
    public class BehaviorRecord
    {
        public int Id { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        [Required]
        public int AssessedById { get; set; }

        [ForeignKey(nameof(AssessedById))]
        public User? AssessedBy { get; set; }

        [Required, MaxLength(200)]
        public string Temperament { get; set; } = string.Empty;

        [Required]
        public bool CompatibilityWithKids { get; set; }

        [Required]
        public bool CompatibilityWithPets { get; set; }

        [MaxLength(300)]
        public string? SpecialRequirements { get; set; }

        public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
    }
}
