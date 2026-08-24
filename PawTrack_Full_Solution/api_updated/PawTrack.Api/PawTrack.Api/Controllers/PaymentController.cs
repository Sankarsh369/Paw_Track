using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawTrack.Api.DTOs.Payment;
using PawTrack.Api.Services;
using System.Security.Claims;

namespace PawTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST api/payment/adoption-fee
        [HttpPost("adoption-fee")]
        [Authorize(Roles = "Adopter")]
        public async Task<ActionResult<PaymentDto>> InitiateAdoptionFee([FromBody] CreateAdoptionFeePaymentDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                return Ok(await _paymentService.InitiateAdoptionFeeAsync(userId, dto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST api/payment/donation — works for logged-in adopters and guests alike
        [HttpPost("donation")]
        [AllowAnonymous]
        public async Task<ActionResult<PaymentDto>> InitiateDonation([FromBody] CreateDonationDto dto)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
                userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                return Ok(await _paymentService.InitiateDonationAsync(userId, dto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST api/payment/verify — called by the frontend right after Razorpay Checkout
        // succeeds client-side. Signature is verified server-side before trusting it.
        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<ActionResult<PaymentDto>> Verify([FromBody] VerifyPaymentDto dto)
        {
            var result = await _paymentService.VerifyAsync(dto);
            if (result is null) return NotFound();
            return Ok(result);
        }

        // POST api/payment/confirm — manual/admin override, kept for testing
        [HttpPost("confirm")]
        [AllowAnonymous]
        public async Task<ActionResult<PaymentDto>> Confirm([FromBody] ConfirmPaymentDto dto)
        {
            var result = await _paymentService.ConfirmAsync(dto);
            if (result is null) return NotFound();
            return Ok(result);
        }

        // GET api/payment/mine
        [HttpGet("mine")]
        [Authorize]
        public async Task<ActionResult<List<PaymentDto>>> GetMine()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _paymentService.GetForUserAsync(userId));
        }

        // GET api/payment?type=Donation&status=Completed
        [HttpGet]
        [Authorize(Roles = "OrgAdmin,BranchAdmin")]
        public async Task<ActionResult<List<PaymentDto>>> GetAll([FromQuery] string? type, [FromQuery] string? status)
        {
            return Ok(await _paymentService.GetAllAsync(type, status));
        }
    }
}
