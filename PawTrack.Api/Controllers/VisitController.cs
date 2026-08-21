using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawTrack.Api.DTOs.Visit;
using PawTrack.Api.Services;

namespace PawTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VisitController : ControllerBase
    {
        private readonly IVisitService _visitService;

        public VisitController(IVisitService visitService)
        {
            _visitService = visitService;
        }

        // ---- VisitSlot Endpoints ----

        // GET api/visit/slots/branch/5
        [HttpGet("slots/branch/{branchId}")]
        public async Task<ActionResult<List<VisitSlotDto>>> GetSlotsByBranch(int branchId)
        {
            var slots = await _visitService.GetSlotsByBranchAsync(branchId);
            return Ok(slots);
        }

        // GET api/visit/slots/5
        [HttpGet("slots/{id}")]
        public async Task<ActionResult<VisitSlotDto>> GetSlotById(int id)
        {
            var slot = await _visitService.GetSlotByIdAsync(id);
            if (slot is null) return NotFound();
            return Ok(slot);
        }

        // POST api/visit/slots
        [HttpPost("slots")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin")]
        public async Task<ActionResult<VisitSlotDto>> CreateSlot([FromBody] CreateVisitSlotDto dto)
        {
            try
            {
                var created = await _visitService.CreateSlotAsync(dto);
                return CreatedAtAction(nameof(GetSlotById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/visit/slots/5
        [HttpDelete("slots/{id}")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var success = await _visitService.DeleteSlotAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // ---- VisitBooking Endpoints ----

        // GET api/visit/bookings/slot/5
        [HttpGet("bookings/slot/{slotId}")]
        [Authorize(Roles = "RescueStaff,BranchAdmin,OrgAdmin")]
        public async Task<ActionResult<List<VisitBookingDto>>> GetBookingsBySlot(int slotId)
        {
            var bookings = await _visitService.GetBookingsBySlotAsync(slotId);
            return Ok(bookings);
        }

        // GET api/visit/bookings/adopter/5
        [HttpGet("bookings/adopter/{adopterId}")]
        public async Task<ActionResult<List<VisitBookingDto>>> GetBookingsByAdopter(int adopterId)
        {
            var bookings = await _visitService.GetBookingsByAdopterAsync(adopterId);
            return Ok(bookings);
        }

        // GET api/visit/bookings/5
        [HttpGet("bookings/{id}")]
        public async Task<ActionResult<VisitBookingDto>> GetBookingById(int id)
        {
            var booking = await _visitService.GetBookingByIdAsync(id);
            if (booking is null) return NotFound();
            return Ok(booking);
        }

        // POST api/visit/bookings
        [HttpPost("bookings")]
        public async Task<ActionResult<VisitBookingDto>> CreateBooking([FromBody] CreateVisitBookingDto dto)
        {
            try
            {
                var created = await _visitService.CreateBookingAsync(dto);
                return CreatedAtAction(nameof(GetBookingById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/visit/bookings/5/status
        [HttpPut("bookings/{id}/status")]
        [Authorize(Roles = "RescueStaff,BranchAdmin,OrgAdmin")]
        public async Task<ActionResult<VisitBookingDto>> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto dto)
        {
            var updated = await _visitService.UpdateBookingStatusAsync(id, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // DELETE api/visit/bookings/5
        [HttpDelete("bookings/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var success = await _visitService.DeleteBookingAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
