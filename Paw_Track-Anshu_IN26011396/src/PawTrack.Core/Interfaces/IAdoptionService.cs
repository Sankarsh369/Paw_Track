using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;

namespace PawTrack.Core.Interfaces;

public interface IAdoptionService
{
    Task<AdoptionApplication> SubmitApplicationAsync(SubmitApplicationDto dto, User currentUser);
    Task<List<ApplicationResponseDto>> GetApplicationsAsync(User currentUser, ApplicationStatus? statusFilter = null, int? branchFilter = null);
    Task<ApplicationResponseDto> GetApplicationByIdAsync(int id, User currentUser);
}
