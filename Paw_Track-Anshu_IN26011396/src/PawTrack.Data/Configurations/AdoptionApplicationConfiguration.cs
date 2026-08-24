using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PawTrack.Core.Entities;

namespace PawTrack.Data.Configurations;

public class AdoptionApplicationConfiguration : IEntityTypeConfiguration<AdoptionApplication>
{
    public void Configure(EntityTypeBuilder<AdoptionApplication> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.RejectionReason).HasMaxLength(500);

        builder.Property(a => a.RowVersion)
            .IsRowVersion();

        // Foreign Key Relationships
        builder.HasOne(a => a.Animal)
            .WithMany(an => an.AdoptionApplications)
            .HasForeignKey(a => a.AnimalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Adopter)
            .WithMany(u => u.AdoptionApplications)
            .HasForeignKey(a => a.AdopterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.VisitBooking)
            .WithMany(v => v.AdoptionApplications)
            .HasForeignKey(a => a.VisitBookingId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Index to help quickly query active pending applications per adopter and animal
        builder.HasIndex(a => new { a.AdopterId, a.AnimalId, a.Status });
    }
}
