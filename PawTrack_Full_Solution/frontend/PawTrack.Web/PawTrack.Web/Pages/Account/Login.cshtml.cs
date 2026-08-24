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
    public class LoginModel : PageModel
    {
        private readonly ApiClient _api;

        public LoginModel(ApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return Page();

            var (success, auth, error) = await _api.PostAsync<AuthResponse>("api/auth/login", new
            {
                email = Input.Email,
                password = Input.Password
            });

            if (!success || auth is null)
            {
                ErrorMessage = error ?? "Invalid email or password.";
                return Page();
            }

            // The API's JWT carries Name/Role/NameIdentifier under short JWT claim
            // names — map them back to standard ClaimTypes for the cookie principal.
            var identity = JwtClaimMapper.ToCookieIdentity(auth.Token, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });

            // Store the raw JWT in a separate HttpOnly cookie so ApiClient can attach it to API calls.
            Response.Cookies.Append(ApiClient.TokenCookieName, auth.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToPage("/Dashboard/Index");
        }
    }
}
