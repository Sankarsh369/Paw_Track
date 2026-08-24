using Microsoft.AspNetCore.Mvc;
using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Core.Interfaces;

namespace PawTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FollowUpController : ControllerBase
{
    private readonly IFollowUpService _followUpService;

    public FollowUpController(IFollowUpService followUpService)
    {
        _followUpService = followUpService;
    }

    private User GetCurrentUser()
    {
        var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
        var roleHeader = Request.Headers["X-User-Role"].FirstOrDefault();
        var branchIdHeader = Request.Headers["X-Branch-Id"].FirstOrDefault();

        if (int.TryParse(userIdHeader, out var userId))
        {
            var role = Enum.TryParse<UserRole>(roleHeader, out var parsedRole) ? parsedRole : UserRole.RescueStaff;
            int? branchId = int.TryParse(branchIdHeader, out var parsedBranchId) ? parsedBranchId : (role == UserRole.OrgAdmin ? null : 1);

            return new User
            {
                Id = userId,
                Name = $"User #{userId}",
                Email = $"user{userId}@pawtrack.org",
                Role = role,
                BranchId = branchId
            };
        }

        return new User
        {
            Id = 3,
            Name = "Alice Staff (Branch 1)",
            Email = "alice.s1@pawtrack.org",
            Role = UserRole.RescueStaff,
            BranchId = 1
        };
    }

    /// <summary>
    /// Schedule a post-adoption follow-up check-in
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<FollowUp>>> CreateFollowUp([FromBody] ScheduleFollowUpDto dto)
    {
        var currentUser = GetCurrentUser();
        try
        {
            var followUp = await _followUpService.ScheduleFollowUpAsync(dto, currentUser);
            return Ok(ApiResponse<FollowUp>.Ok(followUp, "Follow-up check-in scheduled successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<FollowUp>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<FollowUp>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Complete a scheduled follow-up
    /// </summary>
    [HttpPut("{id:int}/complete")]
    public async Task<ActionResult<ApiResponse<FollowUp>>> CompleteFollowUp(int id, [FromBody] CompleteFollowUpDto dto)
    {
        var currentUser = GetCurrentUser();
        try
        {
            var followUp = await _followUpService.CompleteFollowUpAsync(id, dto.NotesUpdate, currentUser);
            return Ok(ApiResponse<FollowUp>.Ok(followUp, "Follow-up marked as completed."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<FollowUp>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<FollowUp>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Get follow-up history for an adoption
    /// </summary>
    [HttpGet("adoption/{adoptionId:int}")]
    public async Task<ActionResult<ApiResponse<List<FollowUpResponseDto>>>> GetFollowUps(int adoptionId)
    {
        var currentUser = GetCurrentUser();
        try
        {
            var history = await _followUpService.GetFollowUpHistoryAsync(adoptionId, currentUser);
            return Ok(ApiResponse<List<FollowUpResponseDto>>.Ok(history));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<List<FollowUpResponseDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<FollowUpResponseDto>>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Get all adoptions with detailed follow-up history
    /// </summary>
    [HttpGet("adoptions")]
    public async Task<ActionResult<ApiResponse<List<AdoptionResponseDto>>>> GetAdoptions()
    {
        var currentUser = GetCurrentUser();
        try
        {
            var adoptions = await _followUpService.GetAdoptionsAsync(currentUser);
            return Ok(ApiResponse<List<AdoptionResponseDto>>.Ok(adoptions));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<AdoptionResponseDto>>.Fail(ex.Message));
        }
    }
}
