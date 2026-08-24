using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PawTrack.Core.Entities;

namespace PawTrack.Data.Configurations;

public class VisitBookingConfiguration : IEntityTypeConfiguration<VisitBooking>
{
    public void Configure(EntityTypeBuilder<VisitBooking> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasOne(v => v.VisitSlot)
            .WithMany(s => s.VisitBookings)
            .HasForeignKey(v => v.VisitSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Animal)
            .WithMany(a => a.VisitBookings)
            .HasForeignKey(v => v.AnimalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Adopter)
            .WithMany(u => u.VisitBookings)
            .HasForeignKey(v => v.AdopterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
