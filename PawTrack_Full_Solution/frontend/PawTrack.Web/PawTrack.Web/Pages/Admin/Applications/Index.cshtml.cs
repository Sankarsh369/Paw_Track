using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;

namespace PawTrack.Web.Pages.Admin.Applications
{
    [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;

        public IndexModel(ApiClient api)
        {
            _api = api;
        }

        public List<AdoptionApplicationModel> Applications { get; set; } = new();
        public string SelectedStatus { get; set; } = "Pending";
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(string? status)
        {
            SelectedStatus = string.IsNullOrWhiteSpace(status) ? "Pending" : status;
            var (success, apps, _) = await _api.GetAsync<List<AdoptionApplicationModel>>($"api/adoption/applications?status={SelectedStatus}");
            Applications = (apps ?? new()).OrderByDescending(a => a.ApplicationDate).ToList();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var (success, error) = await _api.PostAsync($"api/adoption/applications/{id}/review", new { approve = true });
            if (!success) ErrorMessage = error;
            return RedirectToPage(new { status = SelectedStatus });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, string? reason)
        {
            var (success, error) = await _api.PostAsync($"api/adoption/applications/{id}/review", new { approve = false, rejectionReason = reason });
            if (!success) ErrorMessage = error;
            return RedirectToPage(new { status = SelectedStatus });
        }
    }
}
