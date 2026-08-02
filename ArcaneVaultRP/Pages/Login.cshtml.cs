using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
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

            // Attach JWT token from session
            var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
            HttpClientHelper.AttachJwtToken(httpClient, token);

            var users = await httpClient.GetFromJsonAsync<List<ArcaneVaultUser>>("ArcaneVaultUser");

            if (users == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to connect to the server.");
                return Page();
            }

            var user = users.FirstOrDefault(u =>
                u.UserName == Input.Username &&
                u.Password == Input.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }

            if (user.IsDeleted)
            {
                TempData["DisabledUser"] = user.UserName;
                return Page();
            }

            // Store user in session
            HttpContext.Session.SetString(SessionKeys.LoggedInUser,
                JsonSerializer.Serialize(user));
            HttpContext.Session.SetString(SessionKeys.UserRole,
                user.RoleId.ToString());

            // ════════════════════════════════════════════
            // REQUEST AND CACHE JWT TOKEN IN SESSION
            // ════════════════════════════════════════════
            var tokenRes = await httpClient.PostAsJsonAsync("JwtToken", new
            {
                UserName = user.UserName,
                Password = Input.Password
            });

            if (tokenRes.IsSuccessStatusCode)
            {
                var tokenData = await tokenRes.Content.ReadFromJsonAsync<JwtTokenResponse>();
                if (tokenData?.Token != null)
                {
                    HttpContext.Session.SetString(SessionKeys.JwtToken, tokenData.Token);
                }
            }

            // Record login
            await httpClient.PostAsJsonAsync("LoginLog", user.UserName);

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