using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawTrack.Api.Models
{
    public enum AnimalStatus
    {
        UnderAssessment,
        Available,
        Pending,
        Adopted
    }

    public enum Gender
    {
        Male,
        Female,
        Unknown
    }

    // Owned by: Animal & Medical module (Akash)
    public class Animal
    {
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        // The branch (shelter location) currently holding this animal.
        // Required per Database Design doc §5.2 — every Animal belongs to exactly one Branch.
        [Required]
        public int BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string Species { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string Breed { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [MaxLength(300)]
        public string? PhotoUrl { get; set; }

        [Required]
        public DateTime RescueDate { get; set; }

        [Required, MaxLength(200)]
        public string RescueLocation { get; set; } = string.Empty;

        [Required]
        public AnimalStatus Status { get; set; } = AnimalStatus.UnderAssessment;

        // Reserved for future microchip registry integration (System Design §6)
        [MaxLength(50)]
        public string? MicrochipNumber { get; set; }

        // Navigation property to this module's MedicalRecord entity
        public ICollection<MedicalRecord>? MedicalRecords { get; set; }
    }
}
