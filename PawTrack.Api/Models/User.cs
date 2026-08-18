namespace PawTrack.Api.Models
{
    public enum UserRole
    {
        OrgAdmin,
        BranchAdmin,
        RescueStaff,
        Veterinarian,
        Adopter
    }

    public class User
    {
        public int Id { get; set; }
        public int? BranchId { get; set; }   // nullable = null means "org-level" account
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property back to Branch
        public Branch? Branch { get; set; }
    }
}