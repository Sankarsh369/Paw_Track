using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PawTrack.Core.Entities;

namespace PawTrack.Data.Configurations;

public class FollowUpConfiguration : IEntityTypeConfiguration<FollowUp>
{
    public void Configure(EntityTypeBuilder<FollowUp> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Notes).IsRequired();
        builder.Property(f => f.Status).HasConversion<string>().HasMaxLength(30);

        // 1:N Relationship between Adoption and FollowUp
        builder.HasOne(f => f.Adoption)
            .WithMany(a => a.FollowUps)
            .HasForeignKey(f => f.AdoptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.ConductedBy)
            .WithMany(u => u.FollowUpsConducted)
            .HasForeignKey(f => f.ConductedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
