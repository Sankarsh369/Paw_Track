namespace PawTrack.Api.DTOs.Report
{
    public class BranchReportDto
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int TotalAnimals { get; set; }
        public int AvailableAnimals { get; set; }
        public int AdoptedAnimals { get; set; }
        public int PendingApplications { get; set; }
        public int VisitsThisMonth { get; set; }
        public decimal DonationTotal { get; set; }
        public decimal AdoptionFeeTotal { get; set; }
    }

    public class OrgDashboardDto
    {
        public int TotalBranches { get; set; }
        public int TotalAnimals { get; set; }
        public int TotalAdoptions { get; set; }
        public int TotalActiveApplications { get; set; }
        public decimal TotalDonations { get; set; }
        public decimal TotalAdoptionFees { get; set; }
        public List<BranchReportDto> ByBranch { get; set; } = new();
    }
}
