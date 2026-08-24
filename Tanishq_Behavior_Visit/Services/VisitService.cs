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

        public async Task<List<VisitSlotDto>> GetSlotsAsync(int? branchId, DateTime? fromDate)
        {
            var query = _context.VisitSlots.Include(s => s.Branch).AsQueryable();

            if (branchId.HasValue)
                query = query.Where(s => s.BranchId == branchId.Value);

            if (fromDate.HasValue)
                query = query.Where(s => s.SlotDate >= fromDate.Value.Date);

            return await query
                .OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime)
                .Select(s => ToSlotDto(s))
                .ToListAsync();
        }

        public async Task<VisitSlotDto> CreateSlotAsync(CreateVisitSlotDto dto)
        {
            var branchExists = await _context.Branches.AnyAsync(b => b.Id == dto.BranchId);
            if (!branchExists)
                throw new KeyNotFoundException($"Branch with Id {dto.BranchId} was not found.");

            var slot = new VisitSlot
            {
                BranchId = dto.BranchId,
                SlotDate = dto.SlotDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Capacity = dto.Capacity,
                BookedCount = 0
            };

            _context.VisitSlots.Add(slot);
            await _context.SaveChangesAsync();
            await _context.Entry(slot).Reference(s => s.Branch).LoadAsync();

            return ToSlotDto(slot);
        }

        public async Task<bool> DeleteSlotAsync(int id)
        {
            var slot = await _context.VisitSlots.FindAsync(id);
            if (slot is null) return false;

            _context.VisitSlots.Remove(slot);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<VisitBookingDto>> GetBookingsForAdopterAsync(int adopterId)
        {
            return await _context.VisitBookings
                .Include(b => b.VisitSlot)
                .Include(b => b.Animal)
                .Include(b => b.Adopter)
                .Where(b => b.AdopterId == adopterId)
                .OrderByDescending(b => b.BookedAt)
                .Select(b => ToBookingDto(b))
                .ToListAsync();
        }

        public async Task<List<VisitBookingDto>> GetBookingsForBranchAsync(int branchId)
        {
            return await _context.VisitBookings
                .Include(b => b.VisitSlot)
                .Include(b => b.Animal)
                .Include(b => b.Adopter)
                .Where(b => b.VisitSlot!.BranchId == branchId)
                .OrderByDescending(b => b.BookedAt)
                .Select(b => ToBookingDto(b))
                .ToListAsync();
        }

        // Note — per Database Design §5.3: booking a visit never changes Animal.Status.
        // It stays "Available"; multiple adopters may book different slots for the same animal.
        public async Task<VisitBookingDto> BookAsync(int adopterId, CreateVisitBookingDto dto)
        {
            var slot = await _context.VisitSlots.FirstOrDefaultAsync(s => s.Id == dto.VisitSlotId);
            if (slot is null)
                throw new KeyNotFoundException($"Visit slot with Id {dto.VisitSlotId} was not found.");

            var animalExists = await _context.Animals.AnyAsync(a => a.Id == dto.AnimalId);
            if (!animalExists)
                throw new KeyNotFoundException($"Animal with Id {dto.AnimalId} was not found.");

            if (slot.BookedCount >= slot.Capacity)
                throw new InvalidOperationException("This visit slot is fully booked.");

            var booking = new VisitBooking
            {
                VisitSlotId = dto.VisitSlotId,
                AnimalId = dto.AnimalId,
                AdopterId = adopterId,
                Status = VisitBookingStatus.Booked
            };

            slot.BookedCount += 1;

            _context.VisitBookings.Add(booking);
            await _context.SaveChangesAsync();

            await _context.Entry(booking).Reference(b => b.VisitSlot).LoadAsync();
            await _context.Entry(booking).Reference(b => b.Animal).LoadAsync();
            await _context.Entry(booking).Reference(b => b.Adopter).LoadAsync();

            return ToBookingDto(booking);
        }

        public async Task<bool> UpdateBookingStatusAsync(int id, UpdateVisitBookingStatusDto dto)
        {
            var booking = await _context.VisitBookings.FindAsync(id);
            if (booking is null) return false;

            booking.Status = Enum.Parse<VisitBookingStatus>(dto.Status, ignoreCase: true);
            if (dto.Notes is not null) booking.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAsync(int id, int adopterId)
        {
            var booking = await _context.VisitBookings
                .Include(b => b.VisitSlot)
                .FirstOrDefaultAsync(b => b.Id == id && b.AdopterId == adopterId);
            if (booking is null) return false;

            if (booking.Status == VisitBookingStatus.Booked && booking.VisitSlot is not null)
                booking.VisitSlot.BookedCount = Math.Max(0, booking.VisitSlot.BookedCount - 1);

            booking.Status = VisitBookingStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }

        private static VisitSlotDto ToSlotDto(VisitSlot s) => new()
        {
            Id = s.Id,
            BranchId = s.BranchId,
            BranchName = s.Branch?.Name,
            SlotDate = s.SlotDate,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Capacity = s.Capacity,
            BookedCount = s.BookedCount
        };

        private static VisitBookingDto ToBookingDto(VisitBooking b) => new()
        {
            Id = b.Id,
            VisitSlotId = b.VisitSlotId,
            SlotDate = b.VisitSlot?.SlotDate ?? default,
            StartTime = b.VisitSlot?.StartTime ?? default,
            EndTime = b.VisitSlot?.EndTime ?? default,
            AnimalId = b.AnimalId,
            AnimalName = b.Animal?.Name,
            AdopterId = b.AdopterId,
            AdopterName = b.Adopter?.Name,
            Status = b.Status.ToString(),
            BookedAt = b.BookedAt,
            Notes = b.Notes
        };
    }
}
