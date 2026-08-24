using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;
using PawTrack.Api.Models;

namespace PawTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly PawTrackDbContext _db;
        public BranchController(PawTrackDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<Branch>>> GetAll() =>
            Ok(await _db.Branches.ToListAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<Branch>> GetById(int id)
        {
            var branch = await _db.Branches.FindAsync(id);
            if (branch is null) return NotFound();
            return Ok(branch);
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "OrgAdmin")]
        public async Task<ActionResult<Branch>> Create(Branch branch)
        {
            _db.Branches.Add(branch);
            await _db.SaveChangesAsync();
            return Ok(branch);
        }

        [HttpPut("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "OrgAdmin")]
        public async Task<IActionResult> Update(int id, Branch branch)
        {
            var existing = await _db.Branches.FindAsync(id);
            if (existing is null) return NotFound();

            existing.Name = branch.Name;
            existing.RegionCity = branch.RegionCity;
            existing.Address = branch.Address;
            existing.Phone = branch.Phone;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "OrgAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var branch = await _db.Branches.FindAsync(id);
            if (branch is null) return NotFound();

            _db.Branches.Remove(branch);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}