using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace PawTrack.Web.Pages.Admin.Behavior
{
    [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff,Veterinarian")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;

        public IndexModel(ApiClient api)
        {
            _api = api;
        }

        public List<AnimalModel> Animals { get; set; } = new();
        public List<BehaviorRecordModel> Records { get; set; } = new();
        public int? SelectedAnimalId { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required] public int AnimalId { get; set; }
            [Required, MaxLength(200)] public string Temperament { get; set; } = string.Empty;
            public bool CompatibilityWithKids { get; set; }
            public bool CompatibilityWithPets { get; set; }
            [MaxLength(300)] public string? SpecialRequirements { get; set; }
        }

        public async Task OnGetAsync(int? animalId)
        {
            var (s1, animals, _) = await _api.GetAsync<List<AnimalModel>>("api/animal");
            Animals = animals ?? new();

            SelectedAnimalId = animalId ?? Animals.FirstOrDefault()?.Id;
            if (SelectedAnimalId.HasValue)
            {
                var (s2, records, _) = await _api.GetAsync<List<BehaviorRecordModel>>($"api/behavior/animal/{SelectedAnimalId}");
                Records = (records ?? new()).OrderByDescending(r => r.AssessedAt).ToList();
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(Input.AnimalId);
                return Page();
            }

            var (success, _, error) = await _api.PostAsync<BehaviorRecordModel>("api/behavior", new
            {
                animalId = Input.AnimalId,
                temperament = Input.Temperament,
                compatibilityWithKids = Input.CompatibilityWithKids,
                compatibilityWithPets = Input.CompatibilityWithPets,
                specialRequirements = Input.SpecialRequirements
            });

            if (!success)
            {
                ErrorMessage = error;
                await OnGetAsync(Input.AnimalId);
                return Page();
            }

            return RedirectToPage(new { animalId = Input.AnimalId });
        }
    }
}
