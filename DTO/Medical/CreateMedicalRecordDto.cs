using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.Medical
{
    public class CreateMedicalRecordDto
    {
        public int AnimalId { get; set; }

        public int VeterinarianId { get; set; }

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
