using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace PawTrack.Web.Pages.Admin.Medical
{
    [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff,Veterinarian")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;
        private readonly CurrentUser _me;

        public IndexModel(ApiClient api, CurrentUser me)
        {
            _api = api;
            _me = me;
        }

        public List<AnimalModel> Animals { get; set; } = new();
        public List<MedicalRecordModel> Records { get; set; } = new();
        public int? SelectedAnimalId { get; set; }
        public string? ErrorMessage { get; set; }
        public bool CanRecord => _me.IsInAnyRole("Veterinarian", "OrgAdmin", "BranchAdmin");

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required] public int AnimalId { get; set; }
            [Required, MaxLength(500)] public string Diagnosis { get; set; } = string.Empty;
            [MaxLength(500)] public string? Treatment { get; set; }
            [MaxLength(500)] public string? Medication { get; set; }
            public DateTime? VaccinationDate { get; set; }
            [Required] public DateTime CheckupDate { get; set; } = DateTime.UtcNow.Date;
        }

        public async Task OnGetAsync(int? animalId)
        {
            var (s1, animals, _) = await _api.GetAsync<List<AnimalModel>>("api/animal");
            Animals = animals ?? new();

            SelectedAnimalId = animalId ?? Animals.FirstOrDefault()?.Id;
            if (SelectedAnimalId.HasValue)
            {
                var (s2, records, _) = await _api.GetAsync<List<MedicalRecordModel>>($"api/medical/animal/{SelectedAnimalId}");
                Records = (records ?? new()).OrderByDescending(r => r.CheckupDate).ToList();
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(Input.AnimalId);
                return Page();
            }

            var (success, _, error) = await _api.PostAsync<MedicalRecordModel>("api/medical", new
            {
                animalId = Input.AnimalId,
                veterinarianId = _me.UserId,
                diagnosis = Input.Diagnosis,
                treatment = Input.Treatment,
                medication = Input.Medication,
                vaccinationDate = Input.VaccinationDate,
                checkupDate = Input.CheckupDate
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
