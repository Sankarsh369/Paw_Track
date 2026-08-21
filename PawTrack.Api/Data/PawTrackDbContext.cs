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
        public DbSet<BehaviorRecord> BehaviorRecords => Set<BehaviorRecord>();
        public DbSet<VisitSlot> VisitSlots => Set<VisitSlot>();
        public DbSet<VisitBooking> VisitBookings => Set<VisitBooking>();

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

            // ---- Behavior Assessment module (Tanishq) ----

            modelBuilder.Entity<BehaviorRecord>()
                .HasOne(b => b.Animal)
                .WithMany()
                .HasForeignKey(b => b.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BehaviorRecord>()
                .HasOne(b => b.Staff)
                .WithMany()
                .HasForeignKey(b => b.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BehaviorRecord>()
                .Property(b => b.Category)
                .HasConversion<string>();

            // ---- Visit Slots & Bookings module (Tanishq) ----

            modelBuilder.Entity<VisitSlot>()
                .HasOne(s => s.Branch)
                .WithMany()
                .HasForeignKey(s => s.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitSlot>()
                .Property(s => s.Status)
                .HasConversion<string>();

            modelBuilder.Entity<VisitBooking>()
                .HasOne(b => b.VisitSlot)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.VisitSlotId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VisitBooking>()
                .HasOne(b => b.Adopter)
                .WithMany()
                .HasForeignKey(b => b.AdopterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitBooking>()
                .HasOne(b => b.Animal)
                .WithMany()
                .HasForeignKey(b => b.AnimalId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitBooking>()
                .Property(b => b.Status)
                .HasConversion<string>();
        }
    }
}