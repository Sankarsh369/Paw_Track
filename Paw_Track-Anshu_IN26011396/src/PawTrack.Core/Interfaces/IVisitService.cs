using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;

namespace PawTrack.Core.Interfaces;

public interface IVisitService
{
    Task<VisitBooking> BookVisitAsync(BookVisitDto dto, User currentUser);
    Task<VisitBooking> UpdateVisitStatusAsync(int bookingId, VisitBookingStatus status, User currentUser);
    Task<List<VisitSlotResponseDto>> GetVisitSlotsAsync(int branchId);
    Task<List<VisitBookingResponseDto>> GetVisitBookingsAsync(User currentUser);
}
