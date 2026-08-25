using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawTrack.Api.DTOs.AI;
using PawTrack.Api.Services;
using System.Security.Claims;

namespace PawTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiDescriptionController : ControllerBase
    {
        private readonly IAiDescriptionService _aiDescriptionService;

        public AiDescriptionController(IAiDescriptionService aiDescriptionService)
        {
            _aiDescriptionService = aiDescriptionService;
        }

        // POST api/aidescription/generate
        [HttpPost("generate")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<AIGeneratedDescriptionDto>> Generate([FromBody] GenerateDescriptionDto dto)
        {
            try
            {
                return Ok(await _aiDescriptionService.GenerateAsync(dto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GET api/aidescription/pending
        [HttpGet("pending")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<List<AIGeneratedDescriptionDto>>> GetPending()
        {
            return Ok(await _aiDescriptionService.GetPendingAsync());
        }

        // GET api/aidescription/animal/5 — public, approved only
        [HttpGet("animal/{animalId}")]
        [AllowAnonymous]
        public async Task<ActionResult<AIGeneratedDescriptionDto>> GetForAnimal(int animalId)
        {
            var result = await _aiDescriptionService.GetForAnimalAsync(animalId);
            if (result is null) return NotFound();
            return Ok(result);
        }

        // POST api/aidescription/5/review
        [HttpPost("{id}/review")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<AIGeneratedDescriptionDto>> Review(int id, [FromBody] ReviewDescriptionDto dto)
        {
            var reviewerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _aiDescriptionService.ReviewAsync(id, reviewerId, dto);
            if (result is null) return NotFound();
            return Ok(result);
        }
    }
}
