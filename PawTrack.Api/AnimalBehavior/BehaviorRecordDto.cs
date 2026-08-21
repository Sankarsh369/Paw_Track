using PawTrack.Api.Models;

namespace PawTrack.Api.DTOs.Behavior
{
    public class BehaviorRecordDto
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public int StaffId { get; set; }
        public string? StaffName { get; set; }
        public BehaviorCategory Category { get; set; }
        public string Observation { get; set; } = string.Empty;
        public string? Recommendation { get; set; }
        public int? BehaviorScore { get; set; }
        public DateTime AssessmentDate { get; set; }
    }
}
