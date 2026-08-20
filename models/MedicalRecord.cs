using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawTrack.Api.Models
{
    // Owned by: Animal & Medical module (Akash)
    // VeterinarianId references the real User entity (Sankarsha's module) —
    // no separate Veterinarian class, a vet is just a User with Role = Veterinarian.
    public class MedicalRecord
    {
        public int Id { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        [Required]
        public int VeterinarianId { get; set; }

        [ForeignKey(nameof(VeterinarianId))]
        public User? Veterinarian { get; set; }

        [Required, MaxLength(500)]
        public string Diagnosis { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Treatment { get; set; }

        [MaxLength(500)]
        public string? Medication { get; set; }

        public DateTime? VaccinationDate { get; set; }

        [Required]
        public DateTime CheckupDate { get; set; }
    }
}
