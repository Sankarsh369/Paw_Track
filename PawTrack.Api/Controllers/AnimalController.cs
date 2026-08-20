using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawTrack.Api.DTOs.Animal;
using PawTrack.Api.Services;

namespace PawTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnimalController : ControllerBase
    {
        private readonly IAnimalService _animalService;

        public AnimalController(IAnimalService animalService)
        {
            _animalService = animalService;
        }

        // GET api/animal
        [HttpGet]
        public async Task<ActionResult<List<AnimalDto>>> GetAll()
        {
            var animals = await _animalService.GetAllAsync();
            return Ok(animals);
        }

        // GET api/animal/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AnimalDto>> GetById(int id)
        {
            var animal = await _animalService.GetByIdAsync(id);
            if (animal is null) return NotFound();
            return Ok(animal);
        }

        // POST api/animal
        [HttpPost]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<AnimalDto>> Create([FromBody] CreateAnimalDto dto)
        {
            try
            {
                var created = await _animalService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // PUT api/animal/5
        [HttpPut("{id}")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAnimalDto dto)
        {
            var success = await _animalService.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        // DELETE api/animal/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _animalService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
