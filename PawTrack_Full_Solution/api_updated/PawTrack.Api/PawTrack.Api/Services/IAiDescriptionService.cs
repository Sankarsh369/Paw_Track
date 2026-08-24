using PawTrack.Api.DTOs.AI;

namespace PawTrack.Api.Services
{
    public interface IAiDescriptionService
    {
        Task<AIGeneratedDescriptionDto> GenerateAsync(GenerateDescriptionDto dto);
        Task<List<AIGeneratedDescriptionDto>> GetPendingAsync();
        Task<AIGeneratedDescriptionDto?> GetForAnimalAsync(int animalId);
        Task<AIGeneratedDescriptionDto?> ReviewAsync(int id, int reviewerId, ReviewDescriptionDto dto);
    }
}
