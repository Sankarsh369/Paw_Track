namespace PawTrack.Api.DTOs.Medical
{
    public class MedicalRecordDto
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public int VeterinarianId { get; set; }
        public string? VeterinarianName { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string? Treatment { get; set; }
        public string? Medication { get; set; }
        public DateTime? VaccinationDate { get; set; }
        public DateTime CheckupDate { get; set; }
    }
}
