using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.Report;
using PawTrack.Api.Models;

namespace PawTrack.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly PawTrackDbContext _context;

        public ReportService(PawTrackDbContext context)
        {
            _context = context;
        }

        public async Task<OrgDashboardDto> GetOrgDashboardAsync()
        {
            var branches = await _context.Branches
                .AsNoTracking()
                .ToListAsync();
            var byBranch = new List<BranchReportDto>();

            foreach (var branch in branches)
            {
                byBranch.Add(await GetBranchReportAsync(branch.Id));
            }

            return new OrgDashboardDto
            {
                TotalBranches = branches.Count,
                TotalAnimals = byBranch.Sum(b => b.TotalAnimals),
                TotalAdoptions = byBranch.Sum(b => b.AdoptedAnimals),
                TotalActiveApplications = 0,
                TotalDonations = 0,
                TotalAdoptionFees = 0,
                ByBranch = byBranch
            };
        }

        public async Task<BranchReportDto> GetBranchReportAsync(int branchId)
        {
            var branch = await _context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == branchId);
            if (branch is null)
            {
                throw new KeyNotFoundException($"Branch with Id {branchId} was not found.");
            }

            var animals = _context.Animals
                .AsNoTracking()
                .Where(a => a.BranchId == branchId);

            var totalAnimals = await animals.CountAsync();
            var available = await animals.CountAsync(a => a.Status == AnimalStatus.Available);
            var adopted = await animals.CountAsync(a => a.Status == AnimalStatus.Adopted);

            return new BranchReportDto
            {
                BranchId = branch.Id,
                BranchName = branch.Name,
                TotalAnimals = totalAnimals,
                AvailableAnimals = available,
                AdoptedAnimals = adopted,
                PendingApplications = 0,
                VisitsThisMonth = 0,
                DonationTotal = 0,
                AdoptionFeeTotal = 0
            };
        }
    }
}
