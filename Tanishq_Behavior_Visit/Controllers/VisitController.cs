using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawTrack.Api.DTOs.Visit;
using PawTrack.Api.Services;
using System.Security.Claims;

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

        // GET api/visit/slots?branchId=1&fromDate=2026-08-21
        [HttpGet("slots")]
        [AllowAnonymous]
        public async Task<ActionResult<List<VisitSlotDto>>> GetSlots([FromQuery] int? branchId, [FromQuery] DateTime? fromDate)
        {
            var slots = await _visitService.GetSlotsAsync(branchId, fromDate ?? DateTime.UtcNow.Date);
            return Ok(slots);
        }

        // POST api/visit/slots
        [HttpPost("slots")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin")]
        public async Task<ActionResult<VisitSlotDto>> CreateSlot([FromBody] CreateVisitSlotDto dto)
        {
            try
            {
                var created = await _visitService.CreateSlotAsync(dto);
                return Ok(created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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

        // GET api/visit/bookings/mine
        [HttpGet("bookings/mine")]
        public async Task<ActionResult<List<VisitBookingDto>>> GetMyBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _visitService.GetBookingsForAdopterAsync(userId));
        }

        // GET api/visit/bookings/branch/5
        [HttpGet("bookings/branch/{branchId}")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<ActionResult<List<VisitBookingDto>>> GetBranchBookings(int branchId)
        {
            return Ok(await _visitService.GetBookingsForBranchAsync(branchId));
        }

        // POST api/visit/bookings
        [HttpPost("bookings")]
        [Authorize(Roles = "Adopter")]
        public async Task<ActionResult<VisitBookingDto>> Book([FromBody] CreateVisitBookingDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                var booking = await _visitService.BookAsync(userId, dto);
                return Ok(booking);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // PUT api/visit/bookings/5/status
        [HttpPut("bookings/{id}/status")]
        [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateVisitBookingStatusDto dto)
        {
            var success = await _visitService.UpdateBookingStatusAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        // POST api/visit/bookings/5/cancel
        [HttpPost("bookings/{id}/cancel")]
        [Authorize(Roles = "Adopter")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _visitService.CancelAsync(id, userId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
