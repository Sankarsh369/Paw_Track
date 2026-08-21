using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PawTrack.Api.Models
{
    public enum BehaviorCategory
    {
        Aggression,
        Anxiety,
        Socialization,
        Training,
        Feeding,
        General
    }

    // Owned by: Tanishq-DasIN26010306 module
    // StaffId references the real User entity — a staff member is a User with
    // Role = RescueStaff, BranchAdmin, or OrgAdmin.
    public class BehaviorRecord
    {
        public int Id { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        [Required]
        public int StaffId { get; set; }

        [ForeignKey(nameof(StaffId))]
        public User? Staff { get; set; }

        [Required]
        public BehaviorCategory Category { get; set; } = BehaviorCategory.General;

        [Required, MaxLength(1000)]
        public string Observation { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Recommendation { get; set; }

        /// <summary>
        /// Score from 1 (very problematic) to 5 (excellent).
        /// </summary>
        [Range(1, 5)]
        public int? BehaviorScore { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }
    }
}
