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

        [HttpPost]
        public async Task<ActionResult<Branch>> Create(Branch branch)
        {
            _db.Branches.Add(branch);
            await _db.SaveChangesAsync();
            return Ok(branch);
        }
    }
}