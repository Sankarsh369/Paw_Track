namespace PawTrack.Api.DTOs.AiDescription
{
    public class AiAnimalDescriptionDto
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string AnimalName { get; set; } = string.Empty;
        public string GeneratedText { get; set; } = string.Empty;
        public string ModelVersion { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ReviewedById { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
