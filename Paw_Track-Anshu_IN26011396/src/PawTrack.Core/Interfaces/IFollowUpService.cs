using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;

namespace PawTrack.Core.Interfaces;

public interface IFollowUpService
{
    Task<FollowUp> ScheduleFollowUpAsync(ScheduleFollowUpDto dto, User currentUser);
    Task<FollowUp> CompleteFollowUpAsync(int followUpId, string? notesUpdate, User currentUser);
    Task<List<FollowUpResponseDto>> GetFollowUpHistoryAsync(int adoptionId, User currentUser);
    Task<List<AdoptionResponseDto>> GetAdoptionsAsync(User currentUser);
    Task<AdoptionResponseDto> GetAdoptionByIdAsync(int id, User currentUser);
}
