using PawTrack.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.Behavior
{
    public class CreateBehaviorRecordDto
    {
        [Required]
        public int AnimalId { get; set; }

        [Required]
        public int StaffId { get; set; }

        [Required]
        public BehaviorCategory Category { get; set; } = BehaviorCategory.General;

        [Required, MaxLength(1000)]
        public string Observation { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Recommendation { get; set; }

        [Range(1, 5)]
        public int? BehaviorScore { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }
    }
}
