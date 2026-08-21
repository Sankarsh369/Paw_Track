using PawTrack.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.Visit
{
    public class VisitSlotDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime SlotStart { get; set; }
        public DateTime SlotEnd { get; set; }
        public int Capacity { get; set; }
        public int BookingsCount { get; set; }
        public SlotStatus Status { get; set; }
    }

    public class CreateVisitSlotDto
    {
        [Required]
        public int BranchId { get; set; }

        [Required]
        public DateTime SlotStart { get; set; }

        [Required]
        public DateTime SlotEnd { get; set; }

        [Required, Range(1, 100)]
        public int Capacity { get; set; } = 1;
    }
}
