using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawTrack.Api.DTOs.Behavior;
using PawTrack.Api.Services;

namespace PawTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BehaviorController : ControllerBase
    {
        private readonly IBehaviorService _behaviorService;

        public BehaviorController(IBehaviorService behaviorService)
        {
            _behaviorService = behaviorService;
        }

        // GET api/behavior/animal/5
        [HttpGet("animal/{animalId}")]
        public async Task<ActionResult<List<BehaviorRecordDto>>> GetByAnimal(int animalId)
        {
            var records = await _behaviorService.GetByAnimalIdAsync(animalId);
            return Ok(records);
        }

        // GET api/behavior/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BehaviorRecordDto>> GetById(int id)
        {
            var record = await _behaviorService.GetByIdAsync(id);
            if (record is null) return NotFound();
            return Ok(record);
        }

        // POST api/behavior
        [HttpPost]
        [Authorize(Roles = "RescueStaff,BranchAdmin,OrgAdmin")]
        public async Task<ActionResult<BehaviorRecordDto>> Create([FromBody] CreateBehaviorRecordDto dto)
        {
            try
            {
                var created = await _behaviorService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE api/behavior/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "RescueStaff,BranchAdmin,OrgAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _behaviorService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
