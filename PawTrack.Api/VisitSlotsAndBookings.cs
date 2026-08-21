using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;

namespace PawTrack.Api.Models
{
    public enum SlotStatus
    {
        Available,
        FullyBooked,
        Cancelled
    }

    public enum BookingStatus
    {
        Confirmed,
        Cancelled,
        Completed,
        NoShow
    }

    public class VisitSlot
    {
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }

        [Required]
        public DateTime SlotStart { get; set; }

        [Required]
        public DateTime SlotEnd { get; set; }

        [Required, Range(1, 100)]
        public int Capacity { get; set; } = 1;

        [Required]
        public SlotStatus Status { get; set; } = SlotStatus.Available;

        public ICollection<VisitBooking>? Bookings { get; set; }
    }

    public class VisitBooking
    {
        public int Id { get; set; }

        [Required]
        public int VisitSlotId { get; set; }

        [ForeignKey(nameof(VisitSlotId))]
        public VisitSlot? VisitSlot { get; set; }

        [Required]
        public int AdopterId { get; set; }

        [ForeignKey(nameof(AdopterId))]
        public User? Adopter { get; set; }

        public int? AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

namespace PawTrack.Api.DTOs.Visit
{
    using PawTrack.Api.Models;

    public class VisitSlotDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime SlotStart { get; set; }
        public DateTime SlotEnd { get; set; }
        public int Capacity { get; set; }
        public int BookingsCount { get; set; }
        public SlotStatus Status { get; set; }
    }

    public class CreateVisitSlotDto
    {
        [Required]
        public int BranchId { get; set; }

        [Required]
        public DateTime SlotStart { get; set; }

        [Required]
        public DateTime SlotEnd { get; set; }

        [Required, Range(1, 100)]
        public int Capacity { get; set; } = 1;
    }

    public class VisitBookingDto
    {
        public int Id { get; set; }
        public int VisitSlotId { get; set; }
        public DateTime SlotStart { get; set; }
        public DateTime SlotEnd { get; set; }
        public int AdopterId { get; set; }
        public string? AdopterName { get; set; }
        public int? AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public BookingStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateVisitBookingDto
    {
        [Required]
        public int VisitSlotId { get; set; }

        [Required]
        public int AdopterId { get; set; }

        public int? AnimalId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateBookingStatusDto
    {
        [Required]
        public BookingStatus Status { get; set; }
    }
}

namespace PawTrack.Api.Services
{
    using PawTrack.Api.DTOs.Visit;
    using PawTrack.Api.Models;

    public interface IVisitService
    {
        Task<List<VisitSlotDto>> GetSlotsByBranchAsync(int branchId);
        Task<VisitSlotDto?> GetSlotByIdAsync(int id);
        Task<VisitSlotDto> CreateSlotAsync(CreateVisitSlotDto dto);
        Task<bool> DeleteSlotAsync(int id);

        Task<List<VisitBookingDto>> GetBookingsBySlotAsync(int slotId);
        Task<List<VisitBookingDto>> GetBookingsByAdopterAsync(int adopterId);
        Task<VisitBookingDto?> GetBookingByIdAsync(int id);
        Task<VisitBookingDto> CreateBookingAsync(CreateVisitBookingDto dto);
        Task<VisitBookingDto?> UpdateBookingStatusAsync(int id, UpdateBookingStatusDto dto);
        Task<bool> DeleteBookingAsync(int id);
    }

    public class VisitService : IVisitService
    {
        private readonly PawTrackDbContext _context;

        public VisitService(PawTrackDbContext context)
        {
            _context = context;
        }

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

            _context.Set<VisitSlot>().Remove(slot);
            await _context.SaveChangesAsync();
            return true;
        }

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

            var activeBookingsCount = slot.Bookings?.Count(b => b.Status != BookingStatus.Cancelled) ?? 0;
            if (activeBookingsCount >= slot.Capacity)
            {
                throw new InvalidOperationException("This slot is already fully booked.");
            }

            var adopter = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.AdopterId);
            if (adopter is null || adopter.Role != UserRole.Adopter)
                throw new KeyNotFoundException($"No Adopter user found with Id {dto.AdopterId}.");

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

            activeBookingsCount++;
            if (activeBookingsCount >= slot.Capacity)
            {
                slot.Status = SlotStatus.FullyBooked;
                await _context.SaveChangesAsync();
            }

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

            booking.Status = dto.Status;
            await _context.SaveChangesAsync();

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

namespace PawTrack.Api.Controllers
{
    using PawTrack.Api.DTOs.Visit;
    using PawTrack.Api.Services;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VisitController : ControllerBase
    {
        private readonly IVisitService _visitService;

        public VisitController(IVisitService visitService)
        {
            _visitService = visitService;
        }

        [HttpGet("slots/branch/{branchId}")]
        public async Task<ActionResult<List<VisitSlotDto>>> GetSlotsByBranch(int branchId)
        {
            var slots = await _visitService.GetSlotsByBranchAsync(branchId);
            return Ok(slots);
        }

        [HttpGet("slots/{id}")]
        public async Task<ActionResult<VisitSlotDto>> GetSlotById(int id)
        {
            var slot = await _visitService.GetSlotByIdAsync(id);
            if (slot is null) return NotFound();
            return Ok(slot);
        }

        [HttpPost("slots")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin")]
        public async Task<ActionResult<VisitSlotDto>> CreateSlot([FromBody] CreateVisitSlotDto dto)
        {
            try
            {
                var created = await _visitService.CreateSlotAsync(dto);
                return CreatedAtAction(nameof(GetSlotById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("slots/{id}")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var success = await _visitService.DeleteSlotAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("bookings/slot/{slotId}")]
        [Authorize(Roles = "RescueStaff,BranchAdmin,OrgAdmin")]
        public async Task<ActionResult<List<VisitBookingDto>>> GetBookingsBySlot(int slotId)
        {
            var bookings = await _visitService.GetBookingsBySlotAsync(slotId);
            return Ok(bookings);
        }

        [HttpGet("bookings/adopter/{adopterId}")]
        public async Task<ActionResult<List<VisitBookingDto>>> GetBookingsByAdopter(int adopterId)
        {
            var bookings = await _visitService.GetBookingsByAdopterAsync(adopterId);
            return Ok(bookings);
        }

        [HttpGet("bookings/{id}")]
        public async Task<ActionResult<VisitBookingDto>> GetBookingById(int id)
        {
            var booking = await _visitService.GetBookingByIdAsync(id);
            if (booking is null) return NotFound();
            return Ok(booking);
        }

        [HttpPost("bookings")]
        public async Task<ActionResult<VisitBookingDto>> CreateBooking([FromBody] CreateVisitBookingDto dto)
        {
            try
            {
                var created = await _visitService.CreateBookingAsync(dto);
                return CreatedAtAction(nameof(GetBookingById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("bookings/{id}/status")]
        [Authorize(Roles = "RescueStaff,BranchAdmin,OrgAdmin")]
        public async Task<ActionResult<VisitBookingDto>> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto dto)
        {
            var updated = await _visitService.UpdateBookingStatusAsync(id, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("bookings/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var success = await _visitService.DeleteBookingAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
