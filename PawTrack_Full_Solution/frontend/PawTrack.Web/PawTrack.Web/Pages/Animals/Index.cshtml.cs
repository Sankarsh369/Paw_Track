using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;

namespace PawTrack.Web.Pages.Animals
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;

        public IndexModel(ApiClient api)
        {
            _api = api;
        }

        public List<AnimalModel> Animals { get; set; } = new();
        public List<BranchModel> Branches { get; set; } = new();
        public List<CategoryModel> Categories { get; set; } = new();

        public string? SelectedSpecies { get; set; }
        public int? SelectedBranchId { get; set; }
        public string? SelectedStatus { get; set; }

        public async Task OnGetAsync(string? species, int? branchId, string? status)
        {
            SelectedSpecies = species;
            SelectedBranchId = branchId;
            SelectedStatus = status;

            var (success, animals, _) = await _api.GetAsync<List<AnimalModel>>("api/animal");
            var all = animals ?? new List<AnimalModel>();

            if (!string.IsNullOrWhiteSpace(species))
                all = all.Where(a => string.Equals(a.Species, species, StringComparison.OrdinalIgnoreCase)).ToList();

            if (branchId.HasValue)
                all = all.Where(a => a.BranchId == branchId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(status))
                all = all.Where(a => string.Equals(a.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();
            else
                all = all.Where(a => a.Status != "Adopted").ToList(); // default: hide already-adopted animals

            Animals = all.OrderByDescending(a => a.RescueDate).ToList();

            var (bs, branches, _) = await _api.GetAsync<List<BranchModel>>("api/branch");
            Branches = branches ?? new();

            var (cs, cats, _) = await _api.GetAsync<List<CategoryModel>>("api/category");
            Categories = cats ?? new();
        }
    }
}
