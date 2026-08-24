using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;

namespace PawTrack.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;

        public IndexModel(ApiClient api)
        {
            _api = api;
        }

        public List<AnimalModel> Featured { get; set; } = new();
        public int AvailableCount { get; set; }
        public int BranchCount { get; set; }
        public int AdoptedCount { get; set; }

        public async Task OnGetAsync()
        {
            var (success, animals, _) = await _api.GetAsync<List<AnimalModel>>("api/animal");
            var all = animals ?? new List<AnimalModel>();

            Featured = all.Where(a => a.Status == "Available").Take(3).ToList();
            AvailableCount = all.Count(a => a.Status == "Available");
            AdoptedCount = all.Count(a => a.Status == "Adopted");

            var (bSuccess, branches, _) = await _api.GetAsync<List<BranchModel>>("api/branch");
            BranchCount = branches?.Count ?? 0;
        }
    }
}
