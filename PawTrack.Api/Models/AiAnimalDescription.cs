using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawTrack.Api.Models
{
    public enum AiDescriptionStatus
    {
        Draft,
        Approved,
        Rejected
    }

    public class AiAnimalDescription
    {
        public int Id { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        [Required]
        public string GeneratedText { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string ModelVersion { get; set; } = string.Empty;

        [Required]
        public AiDescriptionStatus Status { get; set; } = AiDescriptionStatus.Draft;

        public int? ReviewedById { get; set; }

        [ForeignKey(nameof(ReviewedById))]
        public User? ReviewedBy { get; set; }

        [MaxLength(500)]
        public string? ReviewNotes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
