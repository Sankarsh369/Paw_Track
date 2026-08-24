using Microsoft.AspNetCore.Authentication.Cookies;
using PawTrack.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// The Web app never talks to the database directly — every read/write goes
// through PawTrack.Api, exactly like a mobile client would (see System Design §… "no schema
// change for a future mobile app" — this project follows the same rule for symmetry).
builder.Services.AddHttpClient<ApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7050/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<CurrentUser>();

// The Web app itself authenticates visitors with a cookie; the JWT issued by
// PawTrack.Api is stored in a second, HttpOnly cookie (see ApiClient.TokenCookieName)
// and is only ever read server-side to call the API — it's never exposed to JS.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
