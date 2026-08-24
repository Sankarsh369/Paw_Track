using Microsoft.EntityFrameworkCore;
using PawTrack.Core.Entities;
using PawTrack.Data.Configurations;

namespace PawTrack.Data;

public class PawTrackDbContext : DbContext
{
    public PawTrackDbContext(DbContextOptions<PawTrackDbContext> options) : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<VisitSlot> VisitSlots => Set<VisitSlot>();
    public DbSet<VisitBooking> VisitBookings => Set<VisitBooking>();
    public DbSet<AdoptionApplication> AdoptionApplications => Set<AdoptionApplication>();
    public DbSet<Adoption> Adoptions => Set<Adoption>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AnimalConfiguration());
        modelBuilder.ApplyConfiguration(new AdoptionApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new AdoptionConfiguration());
        modelBuilder.ApplyConfiguration(new FollowUpConfiguration());
        modelBuilder.ApplyConfiguration(new VisitBookingConfiguration());
    }
}
