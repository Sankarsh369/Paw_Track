namespace PawTrack.Api.Models
{
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RegionCity { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }   // nullable — optional field

        // Navigation property: lets EF Core know one Branch has many Users
        public ICollection<User>? Users { get; set; }
    }
}