using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.Visit;
using PawTrack.Api.Models;

namespace PawTrack.Api.Services
{
    public class VisitService : IVisitService
    {
        private readonly PawTrackDbContext _context;

        public VisitService(PawTrackDbContext context)
        {
            _context = context;
        }

        // ---- VisitSlot Operations ----

        public async Task<List<VisitSlotDto>> GetSlotsByBranchAsync(int branchId)
        {
            return await _context.Set<VisitSlot>()
                .Include(s => s.Branch)
                .Include(s => s.Bookings)
                .Where(s => s.BranchId == branchId)
                .Select(s => ToSlotDto(s))
                .ToListAsync();
        }

        public async Task<VisitSlotDto?> GetSlotByIdAsync(int id)
        {
            var slot = await _context.Set<VisitSlot>()
                .Include(s => s.Branch)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == id);

            return slot is null ? null : ToSlotDto(slot);
        }

        public async Task<VisitSlotDto> CreateSlotAsync(CreateVisitSlotDto dto)
        {
            var branchExists = await _context.Branches.AnyAsync(b => b.Id == dto.BranchId);
            if (!branchExists)
                throw new KeyNotFoundException($"Branch with Id {dto.BranchId} was not found.");

            if (dto.SlotStart >= dto.SlotEnd)
                throw new ArgumentException("Slot start time must be before slot end time.");

            var slot = new VisitSlot
            {
                BranchId = dto.BranchId,
                SlotStart = dto.SlotStart,
                SlotEnd = dto.SlotEnd,
                Capacity = dto.Capacity,
                Status = SlotStatus.Available
            };

            _context.Set<VisitSlot>().Add(slot);
            await _context.SaveChangesAsync();

            await _context.Entry(slot).Reference(s => s.Branch).LoadAsync();

            return ToSlotDto(slot);
        }

        public async Task<bool> DeleteSlotAsync(int id)
        {
            var slot = await _context.Set<VisitSlot>()
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (slot is null) return false;

            // Remove slot and cascade delete/restrict its bookings
            _context.Set<VisitSlot>().Remove(slot);
            await _context.SaveChangesAsync();
            return true;
        }

        // ---- VisitBooking Operations ----

        public async Task<List<VisitBookingDto>> GetBookingsBySlotAsync(int slotId)
        {
            return await _context.Set<VisitBooking>()
                .Include(b => b.VisitSlot)
                .Include(b => b.Adopter)
                .Include(b => b.Animal)
                .Where(b => b.VisitSlotId == slotId)
                .Select(b => ToBookingDto(b))
                .ToListAsync();
        }

        public async Task<List<VisitBookingDto>> GetBookingsByAdopterAsync(int adopterId)
        {
            return await _context.Set<VisitBooking>()
                .Include(b => b.VisitSlot)
                .Include(b => b.Adopter)
                .Include(b => b.Animal)
                .Where(b => b.AdopterId == adopterId)
                .Select(b => ToBookingDto(b))
                .ToListAsync();
        }

        public async Task<VisitBookingDto?> GetBookingByIdAsync(int id)
        {
            var booking = await _context.Set<VisitBooking>()
                .Include(b => b.VisitSlot)
                .Include(b => b.Adopter)
                .Include(b => b.Animal)
                .FirstOrDefaultAsync(b => b.Id == id);

            return booking is null ? null : ToBookingDto(booking);
        }

        public async Task<VisitBookingDto> CreateBookingAsync(CreateVisitBookingDto dto)
        {
            var slot = await _context.Set<VisitSlot>()
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == dto.VisitSlotId);

            if (slot is null)
                throw new KeyNotFoundException($"VisitSlot with Id {dto.VisitSlotId} was not found.");

            if (slot.Status == SlotStatus.Cancelled)
                throw new InvalidOperationException("Cannot book a cancelled time slot.");

            // Check capacity
            var activeBookingsCount = slot.Bookings?.Count(b => b.Status != BookingStatus.Cancelled) ?? 0;
            if (activeBookingsCount >= slot.Capacity)
            {
                throw new InvalidOperationException("This slot is already fully booked.");
            }

            // Verify Adopter user exists
            var adopter = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.AdopterId);
            if (adopter is null || adopter.Role != UserRole.Adopter)
                throw new KeyNotFoundException($"No Adopter user found with Id {dto.AdopterId}.");

            // Verify Animal if animalId is specified
            if (dto.AnimalId.HasValue)
            {
                var animalExists = await _context.Animals.AnyAsync(a => a.Id == dto.AnimalId.Value);
                if (!animalExists)
                    throw new KeyNotFoundException($"Animal with Id {dto.AnimalId.Value} was not found.");
            }

            var booking = new VisitBooking
            {
                VisitSlotId = dto.VisitSlotId,
                AdopterId = dto.AdopterId,
                AnimalId = dto.AnimalId,
                Status = BookingStatus.Confirmed,
                Notes = dto.Notes
            };

            _context.Set<VisitBooking>().Add(booking);
            await _context.SaveChangesAsync();

            // If slot is now full, update slot status
            activeBookingsCount++;
            if (activeBookingsCount >= slot.Capacity)
            {
                slot.Status = SlotStatus.FullyBooked;
                await _context.SaveChangesAsync();
            }

            // Load navigation properties for DTO output
            await _context.Entry(booking).Reference(b => b.VisitSlot).LoadAsync();
            await _context.Entry(booking).Reference(b => b.Adopter).LoadAsync();
            if (booking.AnimalId.HasValue)
            {
                await _context.Entry(booking).Reference(b => b.Animal).LoadAsync();
            }

            return ToBookingDto(booking);
        }

        public async Task<VisitBookingDto?> UpdateBookingStatusAsync(int id, UpdateBookingStatusDto dto)
        {
            var booking = await _context.Set<VisitBooking>()
                .Include(b => b.VisitSlot)
                .ThenInclude(s => s!.Bookings)
                .Include(b => b.Adopter)
                .Include(b => b.Animal)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking is null) return null;

            var oldStatus = booking.Status;
            booking.Status = dto.Status;
            await _context.SaveChangesAsync();

            // If booking was cancelled (or reactivated), check/update slot status
            var slot = booking.VisitSlot;
            if (slot is null) return ToBookingDto(booking);

            var activeBookingsCount = slot.Bookings?.Count(b => b.Status != BookingStatus.Cancelled) ?? 0;
            if (activeBookingsCount >= slot.Capacity)
            {
                slot.Status = SlotStatus.FullyBooked;
            }
            else if (slot.Status == SlotStatus.FullyBooked && activeBookingsCount < slot.Capacity)
            {
                slot.Status = SlotStatus.Available;
            }
            await _context.SaveChangesAsync();

            return ToBookingDto(booking);
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            var booking = await _context.Set<VisitBooking>()
                .Include(b => b.VisitSlot)
                .ThenInclude(s => s!.Bookings)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking is null) return false;

            var slot = booking.VisitSlot;
            _context.Set<VisitBooking>().Remove(booking);
            await _context.SaveChangesAsync();

            if (slot is not null)
            {
                var activeBookingsCount = slot.Bookings?.Count(b => b.Status != BookingStatus.Cancelled && b.Id != id) ?? 0;
                if (slot.Status == SlotStatus.FullyBooked && activeBookingsCount < slot.Capacity)
                {
                    slot.Status = SlotStatus.Available;
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        // ---- Mapping Helpers ----

        private static VisitSlotDto ToSlotDto(VisitSlot s) => new()
        {
            Id = s.Id,
            BranchId = s.BranchId,
            BranchName = s.Branch?.Name,
            SlotStart = s.SlotStart,
            SlotEnd = s.SlotEnd,
            Capacity = s.Capacity,
            BookingsCount = s.Bookings?.Count(b => b.Status != BookingStatus.Cancelled) ?? 0,
            Status = s.Status
        };

        private static VisitBookingDto ToBookingDto(VisitBooking b) => new()
        {
            Id = b.Id,
            VisitSlotId = b.VisitSlotId,
            SlotStart = b.VisitSlot?.SlotStart ?? DateTime.MinValue,
            SlotEnd = b.VisitSlot?.SlotEnd ?? DateTime.MinValue,
            AdopterId = b.AdopterId,
            AdopterName = b.Adopter?.Name,
            AnimalId = b.AnimalId,
            AnimalName = b.Animal?.Name,
            Status = b.Status,
            Notes = b.Notes,
            CreatedAt = b.CreatedAt
        };
    }
}
