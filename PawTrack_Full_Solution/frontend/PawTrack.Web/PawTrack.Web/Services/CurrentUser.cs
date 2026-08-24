using System.Security.Claims;

namespace PawTrack.Web.Services
{
    // Small helper so pages don't have to dig through ClaimsPrincipal directly.
    public class CurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

        public string Name => Principal?.FindFirstValue(ClaimTypes.Name) ?? "Guest";

        public string Role => Principal?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        public int? UserId
        {
            get
            {
                var raw = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(raw, out var id) ? id : null;
            }
        }

        public bool IsInAnyRole(params string[] roles) =>
            IsAuthenticated && roles.Any(r => string.Equals(r, Role, StringComparison.OrdinalIgnoreCase));

        public bool IsStaff => IsInAnyRole("OrgAdmin", "BranchAdmin", "RescueStaff", "Veterinarian");
        public bool IsAdmin => IsInAnyRole("OrgAdmin", "BranchAdmin");
    }
}
