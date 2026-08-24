using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawTrack.Api.DTOs.Adoption;
using PawTrack.Api.DTOs.FollowUp;
using PawTrack.Api.Services;
using System.Security.Claims;

namespace PawTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdoptionController : ControllerBase
    {
        private readonly IAdoptionService _adoptionService;

        public AdoptionController(IAdoptionService adoptionService)
        {
            _adoptionService = adoptionService;
        }

        // POST api/adoption/applications
        [HttpPost("applications")]
        [Authorize(Roles = "Adopter")]
        public async Task<ActionResult<AdoptionApplicationDto>> Apply([FromBody] CreateAdoptionApplicationDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                var created = await _adoptionService.ApplyAsync(userId, dto);
                return Ok(created);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }

        // GET api/adoption/applications/mine
        [HttpGet("applications/mine")]
        public async Task<ActionResult<List<AdoptionApplicationDto>>> GetMyApplications()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _adoptionService.GetApplicationsForAdopterAsync(userId));
        }

        // GET api/adoption/applications?branchId=1&status=Pending
        [HttpGet("applications")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<List<AdoptionApplicationDto>>> GetApplications([FromQuery] int? branchId, [FromQuery] string? status)
        {
            return Ok(await _adoptionService.GetApplicationsForBranchAsync(branchId, status));
        }

        // GET api/adoption/applications/5
        [HttpGet("applications/{id}")]
        public async Task<ActionResult<AdoptionApplicationDto>> GetApplication(int id)
        {
            var app = await _adoptionService.GetApplicationByIdAsync(id);
            if (app is null) return NotFound();
            return Ok(app);
        }

        // POST api/adoption/applications/5/review
        [HttpPost("applications/{id}/review")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<AdoptionDto>> Review(int id, [FromBody] ReviewAdoptionApplicationDto dto)
        {
            var reviewerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _adoptionService.ReviewApplicationAsync(id, reviewerId, dto);

            if (!dto.Approve) return NoContent(); // rejected — nothing to return
            if (result is null) return NotFound();
            return Ok(result);
        }

        // GET api/adoption?branchId=1
        [HttpGet]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<List<AdoptionDto>>> GetAll([FromQuery] int? branchId)
        {
            return Ok(await _adoptionService.GetAdoptionsAsync(branchId));
        }

        // GET api/adoption/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AdoptionDto>> GetById(int id)
        {
            var adoption = await _adoptionService.GetAdoptionByIdAsync(id);
            if (adoption is null) return NotFound();
            return Ok(adoption);
        }

        // GET api/adoption/5/followups
        [HttpGet("{id}/followups")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<List<FollowUpDto>>> GetFollowUps(int id)
        {
            return Ok(await _adoptionService.GetFollowUpsForAdoptionAsync(id));
        }

        // POST api/adoption/followups
        [HttpPost("followups")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<FollowUpDto>> CreateFollowUp([FromBody] CreateFollowUpDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                var created = await _adoptionService.CreateFollowUpAsync(userId, dto);
                return Ok(created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
