//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace ArcaneVaultRP.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public bool Registered { get; set; }

        // Input model — keeps login fields separate from page model
        public class LoginInputModel
        {
            [Required(ErrorMessage = "Username is required.")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            // If already logged in, redirect to home
            if (HttpContext.Session.Get(SessionKeys.LoggedInUser) != null)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public IActionResult OnGetLogout()
        {
            // Clear all session keys on logout
            HttpContext.Session.Remove(SessionKeys.LoggedInUser);
            HttpContext.Session.Remove(SessionKeys.UserRole);
            HttpContext.Session.Remove(SessionKeys.JwtToken);
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

            ArcaneVaultUser? user;
            string? jwt;

            try
            {
                // ════════════════════════════════════════════
                // VERIFY CREDENTIALS ON THE API AND GET A JWT
                // The API checks the password itself; the RP never sees stored passwords.
                // ════════════════════════════════════════════
                var tokenRes = await httpClient.PostAsJsonAsync("JwtToken", new
                {
                    UserName = Input.Username,
                    Password = Input.Password
                });

                if (tokenRes.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                    return Page();
                }

                if (tokenRes.StatusCode == HttpStatusCode.Forbidden)
                {
                    // Correct password, but the account is disabled — offer to re-enable it
                    TempData["DisabledUser"] = Input.Username;
                    return Page();
                }

                tokenRes.EnsureSuccessStatusCode();
                jwt = (await tokenRes.Content.ReadFromJsonAsync<JwtTokenResponse>())?.Token;

                // Profile details for the session (the API no longer returns passwords)
                user = await httpClient.GetFromJsonAsync<ArcaneVaultUser>(
                    $"ArcaneVaultUser/{Uri.EscapeDataString(Input.Username)}");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                // API down, asleep, misconfigured URL, or returned something unexpected
                ModelState.AddModelError(string.Empty, "Unable to connect to the server.");
                return Page();
            }

            if (user == null || string.IsNullOrEmpty(jwt))
            {
                ModelState.AddModelError(string.Empty, "Unable to connect to the server.");
                return Page();
            }

            // Store user and JWT in session
            HttpContext.Session.SetString(SessionKeys.LoggedInUser,
                JsonSerializer.Serialize(user));
            HttpContext.Session.SetString(SessionKeys.UserRole,
                user.RoleId.ToString());
            HttpContext.Session.SetString(SessionKeys.JwtToken, jwt);

            // Record login — best effort; a logging failure shouldn't block a successful login
            try
            {
                var authedClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
                HttpClientHelper.AttachJwtToken(authedClient, jwt);
                await authedClient.PostAsJsonAsync("LoginLog", user.UserName);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { }

            return RedirectToPage("/Index");
        }
        public class JwtTokenResponse
        {
            public string Token { get; set; }
            public DateTime TokenExpiration { get; set; }
            public string? UserName { get; set; }
            public string? Email { get; set; }
            public string? RoleName { get; set; }
        }
    }
}