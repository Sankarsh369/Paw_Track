using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.Adoption
{
    public class AdoptionApplicationDto
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public int AdopterId { get; set; }
        public string? AdopterName { get; set; }
        public int? VisitBookingId { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }

    public class CreateAdoptionApplicationDto
    {
        [Required]
        public int AnimalId { get; set; }

        public int? VisitBookingId { get; set; }
    }

    public class ReviewAdoptionApplicationDto
    {
        [Required]
        public bool Approve { get; set; }

        [MaxLength(400)]
        public string? RejectionReason { get; set; }
    }

    public class AdoptionDto
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public int AdopterId { get; set; }
        public string? AdopterName { get; set; }
        public int ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime AdoptionDate { get; set; }
    }
}
