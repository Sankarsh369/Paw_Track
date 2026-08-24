using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawTrack.Api.Models
{
    public enum AdoptionApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum FollowUpStatus
    {
        Scheduled,
        Completed,
        Missed
    }

    // Adoption Management module
    public class AdoptionApplication
    {
        public int Id { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        [Required]
        public int AdopterId { get; set; }

        [ForeignKey(nameof(AdopterId))]
        public User? Adopter { get; set; }

        // Optional — set when this application follows a visit
        public int? VisitBookingId { get; set; }

        [ForeignKey(nameof(VisitBookingId))]
        public VisitBooking? VisitBooking { get; set; }

        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        [Required]
        public AdoptionApplicationStatus Status { get; set; } = AdoptionApplicationStatus.Pending;

        [MaxLength(400)]
        public string? RejectionReason { get; set; }

        public Adoption? Adoption { get; set; }
    }

    // Adopter Management module — the finalized adoption once an application is approved
    public class Adoption
    {
        public int Id { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [ForeignKey(nameof(ApplicationId))]
        public AdoptionApplication? Application { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        [Required]
        public int AdopterId { get; set; }

        [ForeignKey(nameof(AdopterId))]
        public User? Adopter { get; set; }

        [Required]
        public int ApprovedById { get; set; }

        [ForeignKey(nameof(ApprovedById))]
        public User? ApprovedBy { get; set; }

        [Required]
        public DateTime AdoptionDate { get; set; }

        public ICollection<FollowUp>? FollowUps { get; set; }
        public Payment? Payment { get; set; }
    }

    // Admin & Reports module — post-adoption follow-up
    public class FollowUp
    {
        public int Id { get; set; }

        [Required]
        public int AdoptionId { get; set; }

        [ForeignKey(nameof(AdoptionId))]
        public Adoption? Adoption { get; set; }

        [Required]
        public int ConductedById { get; set; }

        [ForeignKey(nameof(ConductedById))]
        public User? ConductedBy { get; set; }

        [Required]
        public DateTime FollowUpDate { get; set; }

        [Required, MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        public FollowUpStatus Status { get; set; } = FollowUpStatus.Scheduled;
    }
}
