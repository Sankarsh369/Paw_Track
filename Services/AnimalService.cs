using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.Animal;
using PawTrack.Api.Models;

namespace PawTrack.Api.Services
{
    public class AnimalService : IAnimalService
    {
        private readonly PawTrackDbContext _context;

        public AnimalService(PawTrackDbContext context)
        {
            _context = context;
        }

        public async Task<List<AnimalDto>> GetAllAsync()
        {
            return await _context.Animals
                .Include(a => a.Category)
                .Select(a => ToDto(a))
                .ToListAsync();
        }

        public async Task<AnimalDto?> GetByIdAsync(int id)
        {
            var animal = await _context.Animals
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id);

            return animal is null ? null : ToDto(animal);
        }

        public async Task<AnimalDto> CreateAsync(CreateAnimalDto dto)
        {
            var animal = new Animal
            {
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                Species = dto.Species,
                Breed = dto.Breed,
                Age = dto.Age,
                Gender = Enum.Parse<Gender>(dto.Gender, ignoreCase: true),
                PhotoUrl = dto.PhotoUrl,
                RescueDate = dto.RescueDate,
                RescueLocation = dto.RescueLocation,
                MicrochipNumber = dto.MicrochipNumber,
                Status = AnimalStatus.UnderAssessment
            };

            _context.Animals.Add(animal);
            await _context.SaveChangesAsync();

            return ToDto(animal);
        }

        public async Task<bool> UpdateAsync(int id, UpdateAnimalDto dto)
        {
            var animal = await _context.Animals.FindAsync(id);
            if (animal is null) return false;

            animal.Name = dto.Name;
            animal.Species = dto.Species;
            animal.Breed = dto.Breed;
            animal.Age = dto.Age;
            animal.Gender = Enum.Parse<Gender>(dto.Gender, ignoreCase: true);
            animal.PhotoUrl = dto.PhotoUrl;
            animal.Status = Enum.Parse<AnimalStatus>(dto.Status, ignoreCase: true);
            animal.MicrochipNumber = dto.MicrochipNumber;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var animal = await _context.Animals.FindAsync(id);
            if (animal is null) return false;

            _context.Animals.Remove(animal);
            await _context.SaveChangesAsync();
            return true;
        }

        private static AnimalDto ToDto(Animal a) => new()
        {
            Id = a.Id,
            CategoryId = a.CategoryId,
            CategoryName = a.Category?.Name,
            Name = a.Name,
            Species = a.Species,
            Breed = a.Breed,
            Age = a.Age,
            Gender = a.Gender.ToString(),
            PhotoUrl = a.PhotoUrl,
            RescueDate = a.RescueDate,
            RescueLocation = a.RescueLocation,
            Status = a.Status.ToString(),
            MicrochipNumber = a.MicrochipNumber
        };
    }
}
