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
            var branches = await _context.Branches.ToListAsync();
            var byBranch = new List<BranchReportDto>();

            foreach (var branch in branches)
                byBranch.Add(await GetBranchReportAsync(branch.Id));

            return new OrgDashboardDto
            {
                TotalBranches = branches.Count,
                TotalAnimals = byBranch.Sum(b => b.TotalAnimals),
                TotalAdoptions = byBranch.Sum(b => b.AdoptedAnimals),
                TotalActiveApplications = byBranch.Sum(b => b.PendingApplications),
                TotalDonations = byBranch.Sum(b => b.DonationTotal),
                TotalAdoptionFees = byBranch.Sum(b => b.AdoptionFeeTotal),
                ByBranch = byBranch
            };
        }

        public async Task<BranchReportDto> GetBranchReportAsync(int branchId)
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch is null)
                throw new KeyNotFoundException($"Branch with Id {branchId} was not found.");

            var animals = _context.Animals.Where(a => a.BranchId == branchId);
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var totalAnimals = await animals.CountAsync();
            var available = await animals.CountAsync(a => a.Status == AnimalStatus.Available);
            var adopted = await animals.CountAsync(a => a.Status == AnimalStatus.Adopted);

            var pendingApps = await _context.AdoptionApplications
                .CountAsync(a => a.Animal!.BranchId == branchId && a.Status == AdoptionApplicationStatus.Pending);

            var visitsThisMonth = await _context.VisitBookings
                .CountAsync(v => v.VisitSlot!.BranchId == branchId && v.VisitSlot!.SlotDate >= monthStart);

            var donationTotal = await _context.Payments
                .Where(p => p.Type == PaymentType.Donation && p.Status == PaymentStatus.Completed && p.Animal != null && p.Animal.BranchId == branchId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var feeTotal = await _context.Payments
                .Where(p => p.Type == PaymentType.AdoptionFee && p.Status == PaymentStatus.Completed && p.Adoption != null && p.Adoption.Animal!.BranchId == branchId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return new BranchReportDto
            {
                BranchId = branch.Id,
                BranchName = branch.Name,
                TotalAnimals = totalAnimals,
                AvailableAnimals = available,
                AdoptedAnimals = adopted,
                PendingApplications = pendingApps,
                VisitsThisMonth = visitsThisMonth,
                DonationTotal = donationTotal,
                AdoptionFeeTotal = feeTotal
            };
        }
    }
}
