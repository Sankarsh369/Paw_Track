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
        }
    }
}