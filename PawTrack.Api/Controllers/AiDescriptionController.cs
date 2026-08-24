using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawTrack.Api.DTOs.AiDescription;
using PawTrack.Api.Services;

namespace PawTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiDescriptionController : ControllerBase
    {
        private readonly IAiDescriptionService _aiDescriptionService;

        public AiDescriptionController(IAiDescriptionService aiDescriptionService)
        {
            _aiDescriptionService = aiDescriptionService;
        }

        // POST api/AiDescription/generate/5
        [HttpPost("generate/{animalId}")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<AiAnimalDescriptionDto>> Generate(int animalId)
        {
            try
            {
                var created = await _aiDescriptionService.GenerateDescriptionAsync(animalId);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GET api/AiDescription/animal/5
        // Returns the latest description for the animal
        [HttpGet("animal/{animalId}")]
        public async Task<ActionResult<AiAnimalDescriptionDto>> GetLatestByAnimal(int animalId)
        {
            var description = await _aiDescriptionService.GetLatestByAnimalIdAsync(animalId);
            if (description is null) return NotFound($"No AI description found for animal ID {animalId}.");
            return Ok(description);
        }

        // GET api/AiDescription/animal/5/history
        // Returns historical AI descriptions for the animal
        [HttpGet("animal/{animalId}/history")]
        public async Task<ActionResult<List<AiAnimalDescriptionDto>>> GetHistoryByAnimal(int animalId)
        {
            var history = await _aiDescriptionService.GetHistoryByAnimalIdAsync(animalId);
            return Ok(history);
        }

        // GET api/AiDescription/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AiAnimalDescriptionDto>> GetById(int id)
        {
            var description = await _aiDescriptionService.GetByIdAsync(id);
            if (description is null) return NotFound($"AiDescription with ID {id} was not found.");
            return Ok(description);
        }

        // PUT api/AiDescription/5
        [HttpPut("{id}")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<AiAnimalDescriptionDto>> Update(int id, [FromBody] UpdateAiAnimalDescriptionDto dto)
        {
            var updated = await _aiDescriptionService.UpdateDescriptionAsync(id, dto);
            if (updated is null) return NotFound($"AiDescription with ID {id} was not found.");
            return Ok(updated);
        }

        // POST api/AiDescription/5/approve
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<AiAnimalDescriptionDto>> Approve(int id, [FromBody] ApproveAiAnimalDescriptionDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var reviewerUserId))
            {
                return Unauthorized("Unable to determine reviewer user identity from claims.");
            }

            var approved = await _aiDescriptionService.ApproveDescriptionAsync(id, reviewerUserId, dto);
            if (approved is null) return NotFound($"AiDescription with ID {id} was not found.");
            return Ok(approved);
        }
    }
}
