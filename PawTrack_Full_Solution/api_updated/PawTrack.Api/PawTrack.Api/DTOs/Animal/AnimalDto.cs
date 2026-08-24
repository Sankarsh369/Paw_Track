namespace PawTrack.Api.DTOs.Animal
{
    public class AnimalDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public DateTime RescueDate { get; set; }
        public string RescueLocation { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? MicrochipNumber { get; set; }
    }
}
