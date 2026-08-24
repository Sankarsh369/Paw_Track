using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Models;

namespace PawTrack.Api.Data
{
    public class PawTrackDbContext : DbContext
    {
        public PawTrackDbContext(DbContextOptions<PawTrackDbContext> options)
            : base(options) { }

        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Animal> Animals => Set<Animal>();
        public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
        public DbSet<AiAnimalDescription> AiAnimalDescriptions => Set<AiAnimalDescription>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User.BranchId is optional
            modelBuilder.Entity<User>()
                .HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // No two users can share an email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Store the Role enum as readable text ("Adopter") not a number (4) —
            // makes the database much easier to read/debug
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            // ---- Animal & Medical module (Akash) ----

            // Animal.BranchId is required — every animal belongs to exactly one branch
            modelBuilder.Entity<Animal>()
                .HasOne(a => a.Branch)
                .WithMany()
                .HasForeignKey(a => a.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Animal>()
                .HasOne(a => a.Category)
                .WithMany(c => c.Animals)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Store Gender/Status as readable text, same reasoning as User.Role above
            modelBuilder.Entity<Animal>()
                .Property(a => a.Gender)
                .HasConversion<string>();

            modelBuilder.Entity<Animal>()
                .Property(a => a.Status)
                .HasConversion<string>();

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Animal)
                .WithMany(a => a.MedicalRecords)
                .HasForeignKey(m => m.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Veterinarian)
                .WithMany()
                .HasForeignKey(m => m.VeterinarianId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- AI Description module ----
            modelBuilder.Entity<AiAnimalDescription>()
                .HasOne(d => d.Animal)
                .WithMany()
                .HasForeignKey(d => d.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AiAnimalDescription>()
                .HasOne(d => d.ReviewedBy)
                .WithMany()
                .HasForeignKey(d => d.ReviewedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AiAnimalDescription>()
                .Property(d => d.Status)
                .HasConversion<string>();
        }
    }
}