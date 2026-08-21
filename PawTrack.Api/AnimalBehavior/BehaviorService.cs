using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.Behavior;
using PawTrack.Api.Models;

namespace PawTrack.Api.Services
{
    public class BehaviorService : IBehaviorService
    {
        private readonly PawTrackDbContext _context;

        public BehaviorService(PawTrackDbContext context)
        {
            _context = context;
        }

        public async Task<List<BehaviorRecordDto>> GetByAnimalIdAsync(int animalId)
        {
            return await _context.BehaviorRecords
                .Include(b => b.Staff)
                .Where(b => b.AnimalId == animalId)
                .Select(b => ToDto(b))
                .ToListAsync();
        }

        public async Task<BehaviorRecordDto?> GetByIdAsync(int id)
        {
            var record = await _context.BehaviorRecords
                .Include(b => b.Staff)
                .FirstOrDefaultAsync(b => b.Id == id);

            return record is null ? null : ToDto(record);
        }

        public async Task<BehaviorRecordDto> CreateAsync(CreateBehaviorRecordDto dto)
        {
            var animalExists = await _context.Animals.AnyAsync(a => a.Id == dto.AnimalId);
            if (!animalExists)
                throw new KeyNotFoundException($"Animal with Id {dto.AnimalId} was not found.");

            // Staff must be a real user with a role that can perform assessments
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

            _context.BehaviorRecords.Add(record);
            await _context.SaveChangesAsync();

            // Reload the Staff navigation so ToDto can populate StaffName
            await _context.Entry(record).Reference(r => r.Staff).LoadAsync();

            return ToDto(record);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var record = await _context.BehaviorRecords.FindAsync(id);
            if (record is null) return false;

            _context.BehaviorRecords.Remove(record);
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
