using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace PawTrack.Web.Pages.Admin.Animals
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
        public List<BranchModel> Branches { get; set; } = new();
        public List<CategoryModel> Categories { get; set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required] public int CategoryId { get; set; }
            [Required] public int BranchId { get; set; }
            [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
            [Required, MaxLength(80)] public string Species { get; set; } = string.Empty;
            [Required, MaxLength(80)] public string Breed { get; set; } = string.Empty;
            [Required, Range(0, 40)] public int Age { get; set; }
            [Required] public string Gender { get; set; } = "Unknown";
            [MaxLength(300)] public string? PhotoUrl { get; set; }
            [Required] public DateTime RescueDate { get; set; } = DateTime.UtcNow.Date;
            [Required, MaxLength(200)] public string RescueLocation { get; set; } = string.Empty;
            [MaxLength(50)] public string? MicrochipNumber { get; set; }
        }

        public async Task OnGetAsync()
        {
            var (s1, animals, _) = await _api.GetAsync<List<AnimalModel>>("api/animal");
            Animals = (animals ?? new()).OrderByDescending(a => a.Id).ToList();

            var (s2, branches, _) = await _api.GetAsync<List<BranchModel>>("api/branch");
            Branches = branches ?? new();

            var (s3, cats, _) = await _api.GetAsync<List<CategoryModel>>("api/category");
            Categories = cats ?? new();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var (success, _, error) = await _api.PostAsync<AnimalModel>("api/animal", new
            {
                categoryId = Input.CategoryId,
                branchId = Input.BranchId,
                name = Input.Name,
                species = Input.Species,
                breed = Input.Breed,
                age = Input.Age,
                gender = Input.Gender,
                photoUrl = Input.PhotoUrl,
                rescueDate = Input.RescueDate,
                rescueLocation = Input.RescueLocation,
                microchipNumber = Input.MicrochipNumber
            });

            if (!success)
            {
                ErrorMessage = error;
                await OnGetAsync();
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
        {
            var (s, animal, _) = await _api.GetAsync<AnimalModel>($"api/animal/{id}");
            if (animal is not null)
            {
                await _api.PutAsync($"api/animal/{id}", new
                {
                    name = animal.Name,
                    species = animal.Species,
                    breed = animal.Breed,
                    age = animal.Age,
                    gender = animal.Gender,
                    photoUrl = animal.PhotoUrl,
                    status,
                    microchipNumber = animal.MicrochipNumber
                });
            }
            return RedirectToPage();
        }
    }
}
