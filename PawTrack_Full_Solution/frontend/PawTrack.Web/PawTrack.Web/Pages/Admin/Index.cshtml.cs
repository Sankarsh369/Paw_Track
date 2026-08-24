using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;

namespace PawTrack.Web.Pages.Admin
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

        public OrgDashboardModel? OrgDashboard { get; set; }
        public bool IsOrgAdmin => _me.IsInAnyRole("OrgAdmin");

        public async Task OnGetAsync()
        {
            if (IsOrgAdmin)
            {
                var (success, dashboard, _) = await _api.GetAsync<OrgDashboardModel>("api/report/org");
                OrgDashboard = dashboard;
            }
        }
    }
}
