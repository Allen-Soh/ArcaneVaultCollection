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
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

            // Get all users from API
            var users = await httpClient.GetFromJsonAsync<List<ArcaneVaultUser>>("ArcaneVaultUser");

            if (users == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to connect to the server. Please try again.");
                return Page();
            }

            // Find matching user — username match and not deleted
            var user = users
                .OrderBy(u => u.UserName)
                .FirstOrDefault(u =>
                u.UserName == Input.Username &&
                u.Password == Input.Password &&
                !u.IsDeleted);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }

            // Store user info in session
            HttpContext.Session.SetString(SessionKeys.LoggedInUser, JsonSerializer.Serialize(user));
            HttpContext.Session.SetString(SessionKeys.UserRole, user.RoleId.ToString());

            var loginHttpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
            await loginHttpClient.PostAsJsonAsync("LoginLog", user.UserName);

            return RedirectToPage("/Index");
        }
    }
}