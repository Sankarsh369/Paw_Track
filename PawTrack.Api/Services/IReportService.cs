using PawTrack.Api.DTOs.Report;

namespace PawTrack.Api.Services
{
    public interface IReportService
    {
        Task<OrgDashboardDto> GetOrgDashboardAsync();
        Task<BranchReportDto> GetBranchReportAsync(int branchId);
    }
}
