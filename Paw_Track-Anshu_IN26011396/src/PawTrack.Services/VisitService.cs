using Microsoft.EntityFrameworkCore;
using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Core.Interfaces;
using PawTrack.Data;

namespace PawTrack.Services;

public class VisitService : IVisitService
{
    private readonly PawTrackDbContext _context;

    public VisitService(PawTrackDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Book a visit slot for an animal.
    /// CRITICAL PAWTRACK RULE: Booking a visit DOES NOT change Animal.Status!
    /// The animal remains Available for other adopters.
    /// </summary>
    public async Task<VisitBooking> BookVisitAsync(BookVisitDto dto, User currentUser)
    {
        var slot = await _context.VisitSlots.FirstOrDefaultAsync(s => s.Id == dto.VisitSlotId);
        if (slot == null)
        {
            throw new KeyNotFoundException($"Visit slot with ID {dto.VisitSlotId} was not found.");
        }

        if (slot.BookedCount >= slot.Capacity)
        {
            throw new InvalidOperationException("This visit slot has reached its maximum capacity.");
        }

        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == dto.AnimalId);
        if (animal == null)
        {
            throw new KeyNotFoundException($"Animal with ID {dto.AnimalId} was not found.");
        }

        if (animal.BranchId != slot.BranchId)
        {
            throw new InvalidOperationException("Selected visit slot belongs to a different branch than the animal.");
        }

        var booking = new VisitBooking
        {
            VisitSlotId = dto.VisitSlotId,
            AnimalId = dto.AnimalId,
            AdopterId = currentUser.Id,
            BookingDate = DateTime.UtcNow.Date,
            Status = VisitBookingStatus.Booked
        };

        slot.BookedCount += 1;

        _context.VisitBookings.Add(booking);
        await _context.SaveChangesAsync();

        return booking;
    }

    /// <summary>
    /// Update visit booking status (CheckedIn, Completed, Cancelled, NoShow)
    /// </summary>
    public async Task<VisitBooking> UpdateVisitStatusAsync(int bookingId, VisitBookingStatus status, User currentUser)
    {
        var booking = await _context.VisitBookings
            .Include(b => b.Animal)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            throw new KeyNotFoundException($"Visit booking with ID {bookingId} was not found.");
        }

        if (currentUser.Role != UserRole.OrgAdmin && booking.Animal != null && currentUser.BranchId != booking.Animal.BranchId)
        {
            throw new UnauthorizedAccessException("Access denied: Staff can only manage visits for their own branch.");
        }

        booking.Status = status;
        await _context.SaveChangesAsync();

        return booking;
    }

    /// <summary>
    /// Get available visit slots for a branch
    /// </summary>
    public async Task<List<VisitSlotResponseDto>> GetVisitSlotsAsync(int branchId)
    {
        var slots = await _context.VisitSlots
            .Where(s => s.BranchId == branchId)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        return slots.Select(s => new VisitSlotResponseDto
        {
            Id = s.Id,
            BranchId = s.BranchId,
            Date = s.Date,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Capacity = s.Capacity,
            BookedCount = s.BookedCount
        }).ToList();
    }

    /// <summary>
    /// Get visit bookings for currentUser or branch
    /// </summary>
    public async Task<List<VisitBookingResponseDto>> GetVisitBookingsAsync(User currentUser)
    {
        var query = _context.VisitBookings
            .Include(b => b.Animal)
            .Include(b => b.Adopter)
            .Include(b => b.VisitSlot)
            .AsQueryable();

        if (currentUser.Role == UserRole.Adopter)
        {
            query = query.Where(b => b.AdopterId == currentUser.Id);
        }
        else if (currentUser.Role != UserRole.OrgAdmin && currentUser.BranchId.HasValue)
        {
            query = query.Where(b => b.Animal != null && b.Animal.BranchId == currentUser.BranchId.Value);
        }

        var bookings = await query.OrderByDescending(b => b.BookingDate).ToListAsync();

        return bookings.Select(b => new VisitBookingResponseDto
        {
            Id = b.Id,
            VisitSlotId = b.VisitSlotId,
            AnimalId = b.AnimalId,
            AnimalName = b.Animal?.Name ?? "Unknown",
            AnimalSpecies = b.Animal?.Species ?? string.Empty,
            AdopterId = b.AdopterId,
            AdopterName = b.Adopter?.Name ?? "Unknown",
            BookingDate = b.BookingDate,
            SlotDate = b.VisitSlot?.Date,
            SlotTime = b.VisitSlot != null
                ? $"{b.VisitSlot.StartTime:hh\\:mm} - {b.VisitSlot.EndTime:hh\\:mm}"
                : string.Empty,
            Status = b.Status,
            StaffNotes = b.StaffNotes
        }).ToList();
    }
}
