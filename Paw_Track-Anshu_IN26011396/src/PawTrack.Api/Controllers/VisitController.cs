using Microsoft.AspNetCore.Mvc;
using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Core.Interfaces;

namespace PawTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitController : ControllerBase
{
    private readonly IVisitService _visitService;

    public VisitController(IVisitService visitService)
    {
        _visitService = visitService;
    }

    private User GetCurrentUser()
    {
        var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
        var roleHeader = Request.Headers["X-User-Role"].FirstOrDefault();

        if (int.TryParse(userIdHeader, out var userId))
        {
            var role = Enum.TryParse<UserRole>(roleHeader, out var parsedRole) ? parsedRole : UserRole.Adopter;
            return new User
            {
                Id = userId,
                Name = $"User #{userId}",
                Email = $"user{userId}@pawtrack.org",
                Role = role,
                BranchId = null
            };
        }

        return new User
        {
            Id = 8,
            Name = "Mark Adopter",
            Email = "mark.adopter@gmail.com",
            Role = UserRole.Adopter,
            BranchId = null
        };
    }

    /// <summary>
    /// Book a visit for an animal
    /// </summary>
    [HttpPost("book")]
    public async Task<ActionResult<ApiResponse<VisitBooking>>> BookVisit([FromBody] BookVisitDto dto)
    {
        var currentUser = GetCurrentUser();
        try
        {
            var booking = await _visitService.BookVisitAsync(dto, currentUser);
            return Ok(ApiResponse<VisitBooking>.Ok(booking, "Visit booked successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<VisitBooking>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Get available visit slots for a branch
    /// </summary>
    [HttpGet("slots/{branchId:int}")]
    public async Task<ActionResult<ApiResponse<List<VisitSlotResponseDto>>>> GetVisitSlots(int branchId)
    {
        try
        {
            var slots = await _visitService.GetVisitSlotsAsync(branchId);
            return Ok(ApiResponse<List<VisitSlotResponseDto>>.Ok(slots));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<VisitSlotResponseDto>>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Get visit bookings for current user/branch
    /// </summary>
    [HttpGet("bookings")]
    public async Task<ActionResult<ApiResponse<List<VisitBookingResponseDto>>>> GetVisitBookings()
    {
        var currentUser = GetCurrentUser();
        try
        {
            var bookings = await _visitService.GetVisitBookingsAsync(currentUser);
            return Ok(ApiResponse<List<VisitBookingResponseDto>>.Ok(bookings));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<VisitBookingResponseDto>>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Update visit status
    /// </summary>
    [HttpPut("bookings/{id:int}/status")]
    public async Task<ActionResult<ApiResponse<VisitBooking>>> UpdateVisitStatus(int id, [FromBody] VisitBookingStatus status)
    {
        var currentUser = GetCurrentUser();
        try
        {
            var booking = await _visitService.UpdateVisitStatusAsync(id, status, currentUser);
            return Ok(ApiResponse<VisitBooking>.Ok(booking, "Visit booking status updated."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<VisitBooking>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<VisitBooking>.Fail(ex.Message));
        }
    }
}
