using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;

namespace PawTrack.Web.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;
        private readonly CurrentUser _me;

        public IndexModel(ApiClient api, CurrentUser me)
        {
            _api = api;
            _me = me;
        }

        public List<VisitBookingModel> Visits { get; set; } = new();
        public List<AdoptionApplicationModel> Applications { get; set; } = new();
        public List<PaymentModel> Payments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (_me.IsStaff)
                return RedirectToPage("/Admin/Index");

            var (vs, visits, _) = await _api.GetAsync<List<VisitBookingModel>>("api/visit/bookings/mine");
            Visits = visits ?? new();

            var (as_, apps, _) = await _api.GetAsync<List<AdoptionApplicationModel>>("api/adoption/applications/mine");
            Applications = apps ?? new();

            var (ps, payments, _) = await _api.GetAsync<List<PaymentModel>>("api/payment/mine");
            Payments = payments ?? new();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelVisitAsync(int bookingId)
        {
            await _api.PostAsync($"api/visit/bookings/{bookingId}/cancel", null);
            return RedirectToPage();
        }
    }
}
