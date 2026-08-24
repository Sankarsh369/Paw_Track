using PawTrack.Api.DTOs.Adoption;
using PawTrack.Api.DTOs.FollowUp;

namespace PawTrack.Api.Services
{
    public interface IAdoptionService
    {
        Task<AdoptionApplicationDto> ApplyAsync(int adopterId, CreateAdoptionApplicationDto dto);
        Task<List<AdoptionApplicationDto>> GetApplicationsForAdopterAsync(int adopterId);
        Task<List<AdoptionApplicationDto>> GetApplicationsForBranchAsync(int? branchId, string? status);
        Task<AdoptionApplicationDto?> GetApplicationByIdAsync(int id);
        Task<AdoptionDto?> ReviewApplicationAsync(int applicationId, int reviewerId, ReviewAdoptionApplicationDto dto);

        Task<List<AdoptionDto>> GetAdoptionsAsync(int? branchId);
        Task<AdoptionDto?> GetAdoptionByIdAsync(int id);

        Task<List<FollowUpDto>> GetFollowUpsForAdoptionAsync(int adoptionId);
        Task<FollowUpDto> CreateFollowUpAsync(int conductedById, CreateFollowUpDto dto);
    }
}
