using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawTrack.Web.Models;
using PawTrack.Web.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PawTrack.Web.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly ApiClient _api;

        public RegisterModel(ApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required, MaxLength(100)]
            public string Name { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password), MinLength(8, ErrorMessage = "Use at least 8 characters.")]
            public string Password { get; set; } = string.Empty;

            [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords don't match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Public self-registration is always as an Adopter — staff/vet accounts
            // are created by an Org Admin from the Staff Console, not exposed here.
            var (success, auth, error) = await _api.PostAsync<AuthResponse>("api/auth/register", new
            {
                name = Input.Name,
                email = Input.Email,
                password = Input.Password,
                role = "Adopter"
            });

            if (!success || auth is null)
            {
                ErrorMessage = error ?? "Couldn't create your account.";
                return Page();
            }

            var identity = JwtClaimMapper.ToCookieIdentity(auth.Token, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });

            Response.Cookies.Append(ApiClient.TokenCookieName, auth.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return RedirectToPage("/Dashboard/Index");
        }
    }
}
