using PawTrack.Api.DTOs.Medical;

namespace PawTrack.Api.Services
{
    public interface IMedicalService
    {
        Task<List<MedicalRecordDto>> GetByAnimalIdAsync(int animalId);
        Task<MedicalRecordDto?> GetByIdAsync(int id);
        Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
