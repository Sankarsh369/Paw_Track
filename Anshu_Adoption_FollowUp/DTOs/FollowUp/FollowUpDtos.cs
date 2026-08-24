using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.FollowUp
{
    public class FollowUpDto
    {
        public int Id { get; set; }
        public int AdoptionId { get; set; }
        public string? AnimalName { get; set; }
        public string? AdopterName { get; set; }
        public int ConductedById { get; set; }
        public string? ConductedByName { get; set; }
        public DateTime FollowUpDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CreateFollowUpDto
    {
        [Required]
        public int AdoptionId { get; set; }

        [Required]
        public DateTime FollowUpDate { get; set; }

        [Required, MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Scheduled";
    }
}
