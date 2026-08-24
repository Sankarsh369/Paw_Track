using PawTrack.Api.DTOs.Animal;

namespace PawTrack.Api.Services
{
    public interface IAnimalService
    {
        Task<List<AnimalDto>> GetAllAsync();
        Task<AnimalDto?> GetByIdAsync(int id);
        Task<AnimalDto> CreateAsync(CreateAnimalDto dto);
        Task<bool> UpdateAsync(int id, UpdateAnimalDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

