using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PawTrack.Core.Entities;

namespace PawTrack.Data.Configurations;

public class AdoptionConfiguration : IEntityTypeConfiguration<Adoption>
{
    public void Configure(EntityTypeBuilder<Adoption> builder)
    {
        builder.HasKey(a => a.Id);

        // 1:1 with Application
        builder.HasOne(a => a.Application)
            .WithOne(app => app.Adoption)
            .HasForeignKey<Adoption>(a => a.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1:1 with Animal
        builder.HasOne(a => a.Animal)
            .WithOne(an => an.Adoption)
            .HasForeignKey<Adoption>(a => a.AnimalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Adopter
        builder.HasOne(a => a.Adopter)
            .WithMany(u => u.AdoptionsAsAdopter)
            .HasForeignKey(a => a.AdopterId)
            .OnDelete(DeleteBehavior.Restrict);

        // ApprovedBy Staff
        builder.HasOne(a => a.ApprovedBy)
            .WithMany(u => u.AdoptionsApproved)
            .HasForeignKey(a => a.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
