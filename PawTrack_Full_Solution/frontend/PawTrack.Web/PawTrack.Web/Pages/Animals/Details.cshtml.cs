using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;

namespace PawTrack.Web.Pages.Animals
{
    [AllowAnonymous]
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _api;
        private readonly CurrentUser _me;

        public DetailsModel(ApiClient api, CurrentUser me)
        {
            _api = api;
            _me = me;
        }

        public AnimalModel? Animal { get; set; }
        public List<BehaviorRecordModel> BehaviorRecords { get; set; } = new();
        public AiDescriptionModel? AiDescription { get; set; }
        public List<VisitSlotModel> AvailableSlots { get; set; } = new();
        public string? FeedbackMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await LoadAsync(id);
            if (Animal is null) return RedirectToPage("/Animals/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostBookVisitAsync(int id, int slotId)
        {
            if (!_me.IsAuthenticated)
                return RedirectToPage("/Account/Login", new { returnUrl = $"/Animals/Details/{id}" });

            if (!_me.IsInAnyRole("Adopter"))
            {
                ErrorMessage = "Only adopter accounts can book visits.";
                await LoadAsync(id);
                return Page();
            }

            var (success, error) = await _api.PostAsync("api/visit/bookings", new { visitSlotId = slotId, animalId = id });

            if (success) FeedbackMessage = "Visit booked! You'll find it under your dashboard.";
            else ErrorMessage = error ?? "Couldn't book that slot — it may have just filled up.";

            await LoadAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostApplyAsync(int id)
        {
            if (!_me.IsAuthenticated)
                return RedirectToPage("/Account/Login", new { returnUrl = $"/Animals/Details/{id}" });

            if (!_me.IsInAnyRole("Adopter"))
            {
                ErrorMessage = "Only adopter accounts can apply to adopt.";
                await LoadAsync(id);
                return Page();
            }

            var (success, error) = await _api.PostAsync("api/adoption/applications", new { animalId = id });

            if (success) FeedbackMessage = "Application submitted! Staff will review it and follow up with you.";
            else ErrorMessage = error ?? "Couldn't submit that application.";

            await LoadAsync(id);
            return Page();
        }

        private async Task LoadAsync(int id)
        {
            var (success, animal, _) = await _api.GetAsync<AnimalModel>($"api/animal/{id}");
            Animal = animal;
            if (Animal is null) return;

            var (bSuccess, behaviors, _) = await _api.GetAsync<List<BehaviorRecordModel>>($"api/behavior/animal/{id}");
            BehaviorRecords = behaviors ?? new();

            var (aSuccess, aiDesc, _) = await _api.GetAsync<AiDescriptionModel>($"api/aidescription/animal/{id}");
            AiDescription = aSuccess ? aiDesc : null;

            var (sSuccess, slots, _) = await _api.GetAsync<List<VisitSlotModel>>($"api/visit/slots?branchId={Animal.BranchId}");
            AvailableSlots = (slots ?? new()).Where(s => !s.IsFull && s.SlotDate >= DateTime.UtcNow.Date).Take(8).ToList();
        }
    }
}
