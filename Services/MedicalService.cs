using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.Medical;
using PawTrack.Api.Models;

namespace PawTrack.Api.Services
{
    public class MedicalService : IMedicalService
    {
        private readonly PawTrackDbContext _context;

        public MedicalService(PawTrackDbContext context)
        {
            _context = context;
        }

        public async Task<List<MedicalRecordDto>> GetByAnimalIdAsync(int animalId)
        {
            return await _context.MedicalRecords
                .Include(m => m.Veterinarian)
                .Where(m => m.AnimalId == animalId)
                .Select(m => ToDto(m))
                .ToListAsync();
        }

        public async Task<MedicalRecordDto?> GetByIdAsync(int id)
        {
            var record = await _context.MedicalRecords
                .Include(m => m.Veterinarian)
                .FirstOrDefaultAsync(m => m.Id == id);

            return record is null ? null : ToDto(record);
        }

        public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto)
        {
            var animalExists = await _context.Animals.AnyAsync(a => a.Id == dto.AnimalId);
            if (!animalExists)
                throw new KeyNotFoundException($"Animal with Id {dto.AnimalId} was not found.");

            // Confirm the given VeterinarianId actually belongs to a User with
            // Role = Veterinarian, not just any user — since User is shared
            // with the whole team, this catches mistaken IDs early.
            var vetIsValid = await _context.Users
                .AnyAsync(u => u.Id == dto.VeterinarianId && u.Role == UserRole.Veterinarian);
            if (!vetIsValid)
                throw new KeyNotFoundException($"No Veterinarian found with Id {dto.VeterinarianId}.");

            var record = new MedicalRecord
            {
                AnimalId = dto.AnimalId,
                VeterinarianId = dto.VeterinarianId,
                Diagnosis = dto.Diagnosis,
                Treatment = dto.Treatment,
                Medication = dto.Medication,
                VaccinationDate = dto.VaccinationDate,
                CheckupDate = dto.CheckupDate
            };

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            return ToDto(record);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record is null) return false;

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }

        private static MedicalRecordDto ToDto(MedicalRecord m) => new()
        {
            Id = m.Id,
            AnimalId = m.AnimalId,
            VeterinarianId = m.VeterinarianId,
            VeterinarianName = m.Veterinarian?.Name,
            Diagnosis = m.Diagnosis,
            Treatment = m.Treatment,
            Medication = m.Medication,
            VaccinationDate = m.VaccinationDate,
            CheckupDate = m.CheckupDate
        };
    }
}
