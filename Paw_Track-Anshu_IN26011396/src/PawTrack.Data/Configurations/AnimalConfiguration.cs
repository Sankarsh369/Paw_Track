using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PawTrack.Core.Entities;

namespace PawTrack.Data.Configurations;

public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Species).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Breed).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(30);

        builder.Property(a => a.RowVersion)
            .IsRowVersion();

        builder.HasOne(a => a.Branch)
            .WithMany(b => b.Animals)
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Category)
            .WithMany(c => c.Animals)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
