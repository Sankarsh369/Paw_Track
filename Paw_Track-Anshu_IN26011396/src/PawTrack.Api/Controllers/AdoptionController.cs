using Microsoft.AspNetCore.Mvc;
using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Core.Interfaces;

namespace PawTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdoptionController : ControllerBase
{
    private readonly IAdoptionService _adoptionService;
    private readonly IApprovalService _approvalService;

    public AdoptionController(IAdoptionService adoptionService, IApprovalService approvalService)
    {
        _adoptionService = adoptionService;
        _approvalService = approvalService;
    }

    /// <summary>
    /// Helper to get current authenticated user from request header or claims
    /// </summary>
    private User GetCurrentUser()
    {
        // For demonstration/evaluation, read user context from header or default to John (Branch 1 Admin)
        var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
        var roleHeader = Request.Headers["X-User-Role"].FirstOrDefault();
        var branchIdHeader = Request.Headers["X-Branch-Id"].FirstOrDefault();

        if (int.TryParse(userIdHeader, out var userId))
        {
            var role = Enum.TryParse<UserRole>(roleHeader, out var parsedRole) ? parsedRole : UserRole.BranchAdmin;
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

        // Default to John Manager (BranchAdmin, Branch 1)
        return new User
        {
            Id = 2,
            Name = "John Manager (Branch 1)",
            Email = "john.b1@pawtrack.org",
            Role = UserRole.BranchAdmin,
            BranchId = 1
        };
    }

    /// <summary>
    /// Submit an adoption application (Adopter)
    /// </summary>
    [HttpPost("applications")]
    public async Task<ActionResult<ApiResponse<AdoptionApplication>>> SubmitApplication([FromBody] SubmitApplicationDto dto)
    {
        var currentUser = GetCurrentUser();
        try
        {
            var application = await _adoptionService.SubmitApplicationAsync(dto, currentUser);
            return Ok(ApiResponse<AdoptionApplication>.Ok(application, "Adoption application submitted successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AdoptionApplication>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Get applications list (Filtered by branch and status)
    /// </summary>
    [HttpGet("applications")]
    public async Task<ActionResult<ApiResponse<List<ApplicationResponseDto>>>> GetApplications(
        [FromQuery] ApplicationStatus? status = null,
        [FromQuery] int? branchId = null)
    {
        var currentUser = GetCurrentUser();
        try
        {
            var applications = await _adoptionService.GetApplicationsAsync(currentUser, status, branchId);
            return Ok(ApiResponse<List<ApplicationResponseDto>>.Ok(applications));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<ApplicationResponseDto>>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Get application by ID
    /// </summary>
    [HttpGet("applications/{id:int}")]
    public async Task<ActionResult<ApiResponse<ApplicationResponseDto>>> GetApplicationById(int id)
    {
        var currentUser = GetCurrentUser();
        try
        {
            var application = await _adoptionService.GetApplicationByIdAsync(id, currentUser);
            return Ok(ApiResponse<ApplicationResponseDto>.Ok(application));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ApplicationResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ApplicationResponseDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Approve an application (Branch Admin / Rescue Staff / Org Admin)
    /// </summary>
    [HttpPost("applications/{id:int}/approve")]
    public async Task<ActionResult<ApiResponse<Adoption>>> ApproveApplication(int id)
    {
        var currentUser = GetCurrentUser();
        try
        {
            var adoption = await _approvalService.ApproveApplicationAsync(id, currentUser);
            return Ok(ApiResponse<Adoption>.Ok(adoption, "Application approved and adoption record created."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<Adoption>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<Adoption>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Reject an application (Branch Admin / Rescue Staff / Org Admin)
    /// </summary>
    [HttpPost("applications/{id:int}/reject")]
    public async Task<ActionResult<ApiResponse>> RejectApplication(int id, [FromBody] RejectApplicationDto dto)
    {
        var currentUser = GetCurrentUser();
        try
        {
            await _approvalService.RejectApplicationAsync(id, dto.RejectionReason, currentUser);
            return Ok(ApiResponse.Ok("Application rejected."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
