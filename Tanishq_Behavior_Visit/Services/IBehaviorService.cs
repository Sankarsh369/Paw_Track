using PawTrack.Api.DTOs.Behavior;

namespace PawTrack.Api.Services
{
    public interface IBehaviorService
    {
        Task<List<BehaviorRecordDto>> GetByAnimalIdAsync(int animalId);
        Task<BehaviorRecordDto?> GetByIdAsync(int id);
        Task<BehaviorRecordDto> CreateAsync(int assessedById, CreateBehaviorRecordDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
