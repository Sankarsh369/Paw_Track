using System.ComponentModel.DataAnnotations;

namespace PawTrack.Api.DTOs.Animal
{
    public class CreateAnimalDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string Species { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string Breed { get; set; } = string.Empty;

        [Required, Range(0, 40)]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? PhotoUrl { get; set; }

        [Required]
        public DateTime RescueDate { get; set; }

        [Required, MaxLength(200)]
        public string RescueLocation { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MicrochipNumber { get; set; }
    }
}
