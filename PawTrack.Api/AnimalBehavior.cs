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
    public enum BehaviorCategory
    {
        Aggression,
        Anxiety,
        Socialization,
        Training,
        Feeding,
        General
    }

    public class BehaviorRecord
    {
        public int Id { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [ForeignKey(nameof(AnimalId))]
        public Animal? Animal { get; set; }

        [Required]
        public int StaffId { get; set; }

        [ForeignKey(nameof(StaffId))]
        public User? Staff { get; set; }

        [Required]
        public BehaviorCategory Category { get; set; } = BehaviorCategory.General;

        [Required, MaxLength(1000)]
        public string Observation { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Recommendation { get; set; }

        [Range(1, 5)]
        public int? BehaviorScore { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }
    }
}

namespace PawTrack.Api.DTOs.Behavior
{
    using PawTrack.Api.Models;

    public class BehaviorRecordDto
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public int StaffId { get; set; }
        public string? StaffName { get; set; }
        public BehaviorCategory Category { get; set; }
        public string Observation { get; set; } = string.Empty;
        public string? Recommendation { get; set; }
        public int? BehaviorScore { get; set; }
        public DateTime AssessmentDate { get; set; }
    }

    public class CreateBehaviorRecordDto
    {
        [Required]
        public int AnimalId { get; set; }

        [Required]
        public int StaffId { get; set; }

        [Required]
        public BehaviorCategory Category { get; set; } = BehaviorCategory.General;

        [Required, MaxLength(1000)]
        public string Observation { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Recommendation { get; set; }

        [Range(1, 5)]
        public int? BehaviorScore { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }
    }
}

namespace PawTrack.Api.Services
{
    using PawTrack.Api.DTOs.Behavior;
    using PawTrack.Api.Models;

    public interface IBehaviorService
    {
        Task<List<BehaviorRecordDto>> GetByAnimalIdAsync(int animalId);
        Task<BehaviorRecordDto?> GetByIdAsync(int id);
        Task<BehaviorRecordDto> CreateAsync(CreateBehaviorRecordDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public class BehaviorService : IBehaviorService
    {
        private readonly PawTrackDbContext _context;

        public BehaviorService(PawTrackDbContext context)
        {
            _context = context;
        }

        public async Task<List<BehaviorRecordDto>> GetByAnimalIdAsync(int animalId)
        {
            return await _context.Set<BehaviorRecord>()
                .Include(b => b.Staff)
                .Where(b => b.AnimalId == animalId)
                .Select(b => ToDto(b))
                .ToListAsync();
        }

        public async Task<BehaviorRecordDto?> GetByIdAsync(int id)
        {
            var record = await _context.Set<BehaviorRecord>()
                .Include(b => b.Staff)
                .FirstOrDefaultAsync(b => b.Id == id);

            return record is null ? null : ToDto(record);
        }

        public async Task<BehaviorRecordDto> CreateAsync(CreateBehaviorRecordDto dto)
        {
            var animalExists = await _context.Animals.AnyAsync(a => a.Id == dto.AnimalId);
            if (!animalExists)
                throw new KeyNotFoundException($"Animal with Id {dto.AnimalId} was not found.");

            var staffIsValid = await _context.Users
                .AnyAsync(u => u.Id == dto.StaffId &&
                               (u.Role == UserRole.RescueStaff ||
                                u.Role == UserRole.BranchAdmin ||
                                u.Role == UserRole.OrgAdmin));
            if (!staffIsValid)
                throw new KeyNotFoundException(
                    $"No RescueStaff/BranchAdmin/OrgAdmin user found with Id {dto.StaffId}.");

            var record = new BehaviorRecord
            {
                AnimalId = dto.AnimalId,
                StaffId = dto.StaffId,
                Category = dto.Category,
                Observation = dto.Observation,
                Recommendation = dto.Recommendation,
                BehaviorScore = dto.BehaviorScore,
                AssessmentDate = dto.AssessmentDate
            };

            _context.Set<BehaviorRecord>().Add(record);
            await _context.SaveChangesAsync();

            await _context.Entry(record).Reference(r => r.Staff).LoadAsync();

            return ToDto(record);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var record = await _context.Set<BehaviorRecord>().FindAsync(id);
            if (record is null) return false;

            _context.Set<BehaviorRecord>().Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }

        private static BehaviorRecordDto ToDto(BehaviorRecord b) => new()
        {
            Id = b.Id,
            AnimalId = b.AnimalId,
            StaffId = b.StaffId,
            StaffName = b.Staff?.Name,
            Category = b.Category,
            Observation = b.Observation,
            Recommendation = b.Recommendation,
            BehaviorScore = b.BehaviorScore,
            AssessmentDate = b.AssessmentDate
        };
    }
}

namespace PawTrack.Api.Controllers
{
    using PawTrack.Api.DTOs.Behavior;
    using PawTrack.Api.Services;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BehaviorController : ControllerBase
    {
        private readonly IBehaviorService _behaviorService;

        public BehaviorController(IBehaviorService behaviorService)
        {
            _behaviorService = behaviorService;
        }

        [HttpGet("animal/{animalId}")]
        public async Task<ActionResult<List<BehaviorRecordDto>>> GetByAnimal(int animalId)
        {
            var records = await _behaviorService.GetByAnimalIdAsync(animalId);
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BehaviorRecordDto>> GetById(int id)
        {
            var record = await _behaviorService.GetByIdAsync(id);
            if (record is null) return NotFound();
            return Ok(record);
        }

        [HttpPost]
        [Authorize(Roles = "RescueStaff,BranchAdmin,OrgAdmin")]
        public async Task<ActionResult<BehaviorRecordDto>> Create([FromBody] CreateBehaviorRecordDto dto)
        {
            try
            {
                var created = await _behaviorService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "RescueStaff,BranchAdmin,OrgAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _behaviorService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
