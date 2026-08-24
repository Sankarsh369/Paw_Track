using PawTrack.Api.DTOs.Visit;

namespace PawTrack.Api.Services
{
    public interface IVisitService
    {
        Task<List<VisitSlotDto>> GetSlotsAsync(int? branchId, DateTime? fromDate);
        Task<VisitSlotDto> CreateSlotAsync(CreateVisitSlotDto dto);
        Task<bool> DeleteSlotAsync(int id);

        Task<List<VisitBookingDto>> GetBookingsForAdopterAsync(int adopterId);
        Task<List<VisitBookingDto>> GetBookingsForBranchAsync(int branchId);
        Task<VisitBookingDto> BookAsync(int adopterId, CreateVisitBookingDto dto);
        Task<bool> UpdateBookingStatusAsync(int id, UpdateVisitBookingStatusDto dto);
        Task<bool> CancelAsync(int id, int adopterId);
    }
}
