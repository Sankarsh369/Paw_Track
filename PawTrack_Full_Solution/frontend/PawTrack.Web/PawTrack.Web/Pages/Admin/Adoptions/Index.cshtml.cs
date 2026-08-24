using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace PawTrack.Web.Pages.Admin.Adoptions
{
    [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;

        public IndexModel(ApiClient api)
        {
            _api = api;
        }

        public List<AdoptionModel> Adoptions { get; set; } = new();

        [BindProperty]
        public FollowUpInput Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public class FollowUpInput
        {
            [Required] public int AdoptionId { get; set; }
            [Required] public DateTime FollowUpDate { get; set; } = DateTime.UtcNow.Date.AddDays(14);
            [Required, MaxLength(500)] public string Notes { get; set; } = string.Empty;
            [Required] public string Status { get; set; } = "Scheduled";
        }

        public async Task OnGetAsync()
        {
            var (success, adoptions, _) = await _api.GetAsync<List<AdoptionModel>>("api/adoption");
            Adoptions = (adoptions ?? new()).OrderByDescending(a => a.AdoptionDate).ToList();
        }

        public async Task<IActionResult> OnPostFollowUpAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var (success, error) = await _api.PostAsync("api/adoption/followups", new
            {
                adoptionId = Input.AdoptionId,
                followUpDate = Input.FollowUpDate,
                notes = Input.Notes,
                status = Input.Status
            });

            if (success) SuccessMessage = "Follow-up logged.";
            else ErrorMessage = error;

            await OnGetAsync();
            return Page();
        }
    }
}
