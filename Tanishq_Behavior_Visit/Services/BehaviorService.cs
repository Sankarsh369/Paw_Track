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
                .Include(b => b.Animal)
                .Include(b => b.AssessedBy)
                .Where(b => b.AnimalId == animalId)
                .Select(b => ToDto(b))
                .ToListAsync();
        }

        public async Task<BehaviorRecordDto?> GetByIdAsync(int id)
        {
            var record = await _context.BehaviorRecords
                .Include(b => b.Animal)
                .Include(b => b.AssessedBy)
                .FirstOrDefaultAsync(b => b.Id == id);

            return record is null ? null : ToDto(record);
        }

        public async Task<BehaviorRecordDto> CreateAsync(int assessedById, CreateBehaviorRecordDto dto)
        {
            var animalExists = await _context.Animals.AnyAsync(a => a.Id == dto.AnimalId);
            if (!animalExists)
                throw new KeyNotFoundException($"Animal with Id {dto.AnimalId} was not found.");

            var record = new BehaviorRecord
            {
                AnimalId = dto.AnimalId,
                AssessedById = assessedById,
                Temperament = dto.Temperament,
                CompatibilityWithKids = dto.CompatibilityWithKids,
                CompatibilityWithPets = dto.CompatibilityWithPets,
                SpecialRequirements = dto.SpecialRequirements
            };

            _context.BehaviorRecords.Add(record);
            await _context.SaveChangesAsync();

            await _context.Entry(record).Reference(b => b.Animal).LoadAsync();
            await _context.Entry(record).Reference(b => b.AssessedBy).LoadAsync();

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
            AnimalName = b.Animal?.Name,
            AssessedById = b.AssessedById,
            AssessedByName = b.AssessedBy?.Name,
            Temperament = b.Temperament,
            CompatibilityWithKids = b.CompatibilityWithKids,
            CompatibilityWithPets = b.CompatibilityWithPets,
            SpecialRequirements = b.SpecialRequirements,
            AssessedAt = b.AssessedAt
        };
    }
}
