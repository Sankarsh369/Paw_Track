using PawTrack.Api.DTOs.AiDescription;

namespace PawTrack.Api.Services
{
    public interface IAiDescriptionService
    {
        Task<AiAnimalDescriptionDto> GenerateDescriptionAsync(int animalId);
        Task<AiAnimalDescriptionDto?> GetLatestByAnimalIdAsync(int animalId);
        Task<List<AiAnimalDescriptionDto>> GetHistoryByAnimalIdAsync(int animalId);
        Task<AiAnimalDescriptionDto?> GetByIdAsync(int id);
        Task<AiAnimalDescriptionDto?> UpdateDescriptionAsync(int id, UpdateAiAnimalDescriptionDto dto);
        Task<AiAnimalDescriptionDto?> ApproveDescriptionAsync(int id, int reviewerUserId, ApproveAiAnimalDescriptionDto dto);
    }
}
