using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace PawTrack.Web.Pages.Admin.Visits
{
    [Authorize(Roles = "OrgAdmin,BranchAdmin,RescueStaff")]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;
        private readonly CurrentUser _me;

        public IndexModel(ApiClient api, CurrentUser me)
        {
            _api = api;
            _me = me;
        }

        public List<BranchModel> Branches { get; set; } = new();
        public List<VisitSlotModel> Slots { get; set; } = new();
        public List<VisitBookingModel> Bookings { get; set; } = new();
        public int? SelectedBranchId { get; set; }
        public string? ErrorMessage { get; set; }
        public bool CanManageSlots => _me.IsInAnyRole("OrgAdmin", "BranchAdmin");

        [BindProperty]
        public SlotInput Input { get; set; } = new();

        public class SlotInput
        {
            [Required] public int BranchId { get; set; }
            [Required] public DateTime SlotDate { get; set; } = DateTime.UtcNow.Date.AddDays(1);
            [Required] public TimeSpan StartTime { get; set; } = new(10, 0, 0);
            [Required] public TimeSpan EndTime { get; set; } = new(11, 0, 0);
            [Required, Range(1, 100)] public int Capacity { get; set; } = 4;
        }

        public async Task OnGetAsync(int? branchId)
        {
            var (s1, branches, _) = await _api.GetAsync<List<BranchModel>>("api/branch");
            Branches = branches ?? new();

            SelectedBranchId = branchId ?? Branches.FirstOrDefault()?.Id;
            if (SelectedBranchId.HasValue)
            {
                var (s2, slots, _) = await _api.GetAsync<List<VisitSlotModel>>($"api/visit/slots?branchId={SelectedBranchId}");
                Slots = (slots ?? new()).OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime).ToList();

                var (s3, bookings, _) = await _api.GetAsync<List<VisitBookingModel>>($"api/visit/bookings/branch/{SelectedBranchId}");
                Bookings = (bookings ?? new()).OrderByDescending(b => b.BookedAt).ToList();
            }
        }

        public async Task<IActionResult> OnPostCreateSlotAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(Input.BranchId);
                return Page();
            }

            var (success, _, error) = await _api.PostAsync<VisitSlotModel>("api/visit/slots", new
            {
                branchId = Input.BranchId,
                slotDate = Input.SlotDate,
                startTime = Input.StartTime,
                endTime = Input.EndTime,
                capacity = Input.Capacity
            });

            if (!success) ErrorMessage = error;
            return RedirectToPage(new { branchId = Input.BranchId });
        }

        public async Task<IActionResult> OnPostDeleteSlotAsync(int slotId, int branchId)
        {
            await _api.DeleteAsync($"api/visit/slots/{slotId}");
            return RedirectToPage(new { branchId });
        }

        public async Task<IActionResult> OnPostMarkAsync(int bookingId, string status, int branchId)
        {
            await _api.PutAsync($"api/visit/bookings/{bookingId}/status", new { status });
            return RedirectToPage(new { branchId });
        }
    }
}
