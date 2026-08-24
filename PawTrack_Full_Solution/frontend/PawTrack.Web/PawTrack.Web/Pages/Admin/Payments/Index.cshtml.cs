using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;

namespace PawTrack.Web.Pages.Admin.Payments
{
    [Authorize(Roles = "OrgAdmin,BranchAdmin")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;

        public IndexModel(ApiClient api)
        {
            _api = api;
        }

        public List<PaymentModel> Payments { get; set; } = new();
        public string? SelectedType { get; set; }
        public decimal TotalDonations { get; set; }
        public decimal TotalFees { get; set; }

        public async Task OnGetAsync(string? type)
        {
            SelectedType = type;
            var query = string.IsNullOrEmpty(type) ? "" : $"?type={type}";
            var (success, payments, _) = await _api.GetAsync<List<PaymentModel>>($"api/payment{query}");
            Payments = (payments ?? new()).OrderByDescending(p => p.PaymentDate).ToList();

            TotalDonations = Payments.Where(p => p.Type == "Donation" && p.Status == "Completed").Sum(p => p.Amount);
            TotalFees = Payments.Where(p => p.Type == "AdoptionFee" && p.Status == "Completed").Sum(p => p.Amount);
        }
    }
}
