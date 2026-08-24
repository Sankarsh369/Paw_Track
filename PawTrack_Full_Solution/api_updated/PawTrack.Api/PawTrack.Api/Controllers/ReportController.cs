using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawTrack.Api.DTOs.Report;
using PawTrack.Api.Services;

namespace PawTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "OrgAdmin,BranchAdmin")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET api/report/org
        [HttpGet("org")]
        [Authorize(Roles = "OrgAdmin")]
        public async Task<ActionResult<OrgDashboardDto>> GetOrgDashboard()
        {
            return Ok(await _reportService.GetOrgDashboardAsync());
        }

        // GET api/report/branch/5
        [HttpGet("branch/{branchId}")]
        public async Task<ActionResult<BranchReportDto>> GetBranchReport(int branchId)
        {
            try
            {
                return Ok(await _reportService.GetBranchReportAsync(branchId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
