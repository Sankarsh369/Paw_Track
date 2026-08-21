using PawTrack.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.Visit
{
    public class VisitBookingDto
    {
        public int Id { get; set; }
        public int VisitSlotId { get; set; }
        public DateTime SlotStart { get; set; }
        public DateTime SlotEnd { get; set; }
        public int AdopterId { get; set; }
        public string? AdopterName { get; set; }
        public int? AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public BookingStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateVisitBookingDto
    {
        [Required]
        public int VisitSlotId { get; set; }

        [Required]
        public int AdopterId { get; set; }

        /// <summary>Optional – the animal the adopter specifically wants to meet.</summary>
        public int? AnimalId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateBookingStatusDto
    {
        [Required]
        public BookingStatus Status { get; set; }
    }
}
