using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace PawTrack.Web.Pages.Admin.Branches
{
    [Authorize(Roles = "OrgAdmin")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;

        public IndexModel(ApiClient api)
        {
            _api = api;
        }

        public List<BranchModel> Branches { get; set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required, MaxLength(120)]
            public string Name { get; set; } = string.Empty;

            [Required, MaxLength(80)]
            public string RegionCity { get; set; } = string.Empty;

            [Required, MaxLength(200)]
            public string Address { get; set; } = string.Empty;

            [MaxLength(30)]
            public string? Phone { get; set; }
        }

        public async Task OnGetAsync()
        {
            var (success, branches, _) = await _api.GetAsync<List<BranchModel>>("api/branch");
            Branches = branches ?? new();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var (success, _, error) = await _api.PostAsync<BranchModel>("api/branch", new
            {
                name = Input.Name,
                regionCity = Input.RegionCity,
                address = Input.Address,
                phone = Input.Phone
            });

            if (!success) ErrorMessage = error;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _api.DeleteAsync($"api/branch/{id}");
            return RedirectToPage();
        }
    }
}
