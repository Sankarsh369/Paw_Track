using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawTrack.Api.DTOs.Medical;
using PawTrack.Api.Services;

namespace PawTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicalController : ControllerBase
    {
        private readonly IMedicalService _medicalService;

        public MedicalController(IMedicalService medicalService)
        {
            _medicalService = medicalService;
        }

        // GET api/medical/animal/5
        [HttpGet("animal/{animalId}")]
        public async Task<ActionResult<List<MedicalRecordDto>>> GetByAnimal(int animalId)
        {
            var records = await _medicalService.GetByAnimalIdAsync(animalId);
            return Ok(records);
        }

        // GET api/medical/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalRecordDto>> GetById(int id)
        {
            var record = await _medicalService.GetByIdAsync(id);
            if (record is null) return NotFound();
            return Ok(record);
        }

        // POST api/medical
        [HttpPost]
        [Authorize(Roles = "Veterinarian,OrgAdmin,BranchAdmin")]
        public async Task<ActionResult<MedicalRecordDto>> Create([FromBody] CreateMedicalRecordDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var vetUserId))
            {
                return Unauthorized("Unable to determine veterinarian user identity from token claims.");
            }

            dto.VeterinarianId = vetUserId;

            try
            {
                var created = await _medicalService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE api/medical/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Veterinarian,OrgAdmin,BranchAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _medicalService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
