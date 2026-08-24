using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;

namespace PawTrack.Web.Pages.Admin.AiDescriptions
{
    [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;

        public IndexModel(ApiClient api)
        {
            _api = api;
        }

        public List<AnimalModel> Animals { get; set; } = new();
        public List<AiDescriptionModel> Pending { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var (s1, animals, _) = await _api.GetAsync<List<AnimalModel>>("api/animal");
            Animals = animals ?? new();

            var (s2, pending, _) = await _api.GetAsync<List<AiDescriptionModel>>("api/aidescription/pending");
            Pending = pending ?? new();
        }

        public async Task<IActionResult> OnPostGenerateAsync(int animalId)
        {
            var (success, _, error) = await _api.PostAsync<AiDescriptionModel>("api/aidescription/generate", new { animalId });
            if (!success) ErrorMessage = error;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id, string finalText)
        {
            var (success, error) = await _api.PostAsync($"api/aidescription/{id}/review", new { finalText, approve = true });
            if (!success) ErrorMessage = error;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDiscardAsync(int id, string finalText)
        {
            var (success, error) = await _api.PostAsync($"api/aidescription/{id}/review", new { finalText, approve = false });
            if (!success) ErrorMessage = error;
            return RedirectToPage();
        }
    }
}
