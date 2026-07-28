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
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
            var users = await httpClient.GetFromJsonAsync<List<ArcaneVaultUser>>("ArcaneVaultUser");

            if (users == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to connect to the server.");
                return Page();
            }

            // Check credentials match — including disabled accounts
            var user = users.FirstOrDefault(u =>
                u.UserName == Input.Username &&
                u.Password == Input.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }

            // Account disabled — return special flag to show re-enable prompt
            if (user.IsDeleted)
            {
                TempData["DisabledUser"] = user.UserName;
                return Page();
            }

            // Success
            HttpContext.Session.SetString(SessionKeys.LoggedInUser, JsonSerializer.Serialize(user));
            HttpContext.Session.SetString(SessionKeys.UserRole, user.RoleId.ToString());

            var loginHttpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
            await loginHttpClient.PostAsJsonAsync("LoginLog", user.UserName);

            return RedirectToPage("/Index");
        }
    }
}