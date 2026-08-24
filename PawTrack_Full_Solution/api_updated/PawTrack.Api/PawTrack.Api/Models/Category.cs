namespace PawTrack.Api.Models
{
    // Owned by: Animal & Medical module.
    // Simple lookup table for species/breed groupings (e.g. "Dog", "Cat", "Bird").
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation property: one Category has many Animals
        public ICollection<Animal>? Animals { get; set; }
    }
}
