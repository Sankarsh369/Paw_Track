using PawTrack.Api.DTOs.Visit;

namespace PawTrack.Api.Services
{
    public interface IVisitService
    {
        // ---- VisitSlot ----
        Task<List<VisitSlotDto>> GetSlotsByBranchAsync(int branchId);
        Task<VisitSlotDto?> GetSlotByIdAsync(int id);
        Task<VisitSlotDto> CreateSlotAsync(CreateVisitSlotDto dto);
        Task<bool> DeleteSlotAsync(int id);

        // ---- VisitBooking ----
        Task<List<VisitBookingDto>> GetBookingsBySlotAsync(int slotId);
        Task<List<VisitBookingDto>> GetBookingsByAdopterAsync(int adopterId);
        Task<VisitBookingDto?> GetBookingByIdAsync(int id);
        Task<VisitBookingDto> CreateBookingAsync(CreateVisitBookingDto dto);
        Task<VisitBookingDto?> UpdateBookingStatusAsync(int id, UpdateBookingStatusDto dto);
        Task<bool> DeleteBookingAsync(int id);
    }
}
