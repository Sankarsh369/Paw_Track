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
        public DbSet<AdoptionApplication> AdoptionApplications => Set<AdoptionApplication>();
        public DbSet<Adoption> Adoptions => Set<Adoption>();
        public DbSet<FollowUp> FollowUps => Set<FollowUp>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<AIGeneratedDescription> AIGeneratedDescriptions => Set<AIGeneratedDescription>();
        public DbSet<ChatbotConversation> ChatbotConversations => Set<ChatbotConversation>();
        public DbSet<ChatbotMessage> ChatbotMessages => Set<ChatbotMessage>();

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

            // ---- Behavior Management module ----
            modelBuilder.Entity<BehaviorRecord>()
                .HasOne(b => b.Animal)
                .WithMany()
                .HasForeignKey(b => b.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BehaviorRecord>()
                .HasOne(b => b.AssessedBy)
                .WithMany()
                .HasForeignKey(b => b.AssessedById)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Visit & Scheduling module ----
            modelBuilder.Entity<VisitSlot>()
                .HasOne(s => s.Branch)
                .WithMany()
                .HasForeignKey(s => s.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitSlot>()
                .Property(s => s.SlotDate).HasColumnType("date");

            modelBuilder.Entity<VisitBooking>()
                .HasOne(b => b.VisitSlot)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.VisitSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitBooking>()
                .HasOne(b => b.Animal)
                .WithMany()
                .HasForeignKey(b => b.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitBooking>()
                .HasOne(b => b.Adopter)
                .WithMany()
                .HasForeignKey(b => b.AdopterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitBooking>()
                .Property(b => b.Status).HasConversion<string>();

            // ---- Adoption Management module ----
            modelBuilder.Entity<AdoptionApplication>()
                .HasOne(a => a.Animal)
                .WithMany()
                .HasForeignKey(a => a.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdoptionApplication>()
                .HasOne(a => a.Adopter)
                .WithMany()
                .HasForeignKey(a => a.AdopterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdoptionApplication>()
                .HasOne(a => a.VisitBooking)
                .WithMany()
                .HasForeignKey(a => a.VisitBookingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdoptionApplication>()
                .Property(a => a.Status).HasConversion<string>();

            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.Application)
                .WithOne(a => a.Adoption)
                .HasForeignKey<Adoption>(a => a.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Adoption>()
                .HasIndex(a => a.ApplicationId)
                .IsUnique();

            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.Animal)
                .WithMany()
                .HasForeignKey(a => a.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.Adopter)
                .WithMany()
                .HasForeignKey(a => a.AdopterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.ApprovedBy)
                .WithMany()
                .HasForeignKey(a => a.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FollowUp>()
                .HasOne(f => f.Adoption)
                .WithMany(a => a.FollowUps)
                .HasForeignKey(f => f.AdoptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FollowUp>()
                .HasOne(f => f.ConductedBy)
                .WithMany()
                .HasForeignKey(f => f.ConductedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FollowUp>()
                .Property(f => f.Status).HasConversion<string>();

            // ---- Payment & Donation module ----
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Adoption)
                .WithOne(a => a.Payment)
                .HasForeignKey<Payment>(p => p.AdoptionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Animal)
                .WithMany()
                .HasForeignKey(p => p.AnimalId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Donor)
                .WithMany()
                .HasForeignKey(p => p.DonorUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .Property(p => p.Type).HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status).HasConversion<string>();

            // ---- AI Description module ----
            modelBuilder.Entity<AIGeneratedDescription>()
                .HasOne(d => d.Animal)
                .WithMany()
                .HasForeignKey(d => d.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AIGeneratedDescription>()
                .HasOne(d => d.ReviewedBy)
                .WithMany()
                .HasForeignKey(d => d.ReviewedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- AI Chatbot module ----
            modelBuilder.Entity<ChatbotConversation>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatbotConversation>()
                .HasIndex(c => c.SessionId);

            modelBuilder.Entity<ChatbotMessage>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatbotMessage>()
                .Property(m => m.Sender).HasConversion<string>();
        }
    }
}