using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultRP.Pages
{
    public class RegistrationModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegistrationModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();

        public class RegisterInputModel
        {
            [Required(ErrorMessage = "Username is required.")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            [StringLength(50, ErrorMessage = "Password must be at least 6 characters long.", MinimumLength = 6)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please confirm your password.")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password does not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.Get(SessionKeys.LoggedInUser) != null)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

            var existingUsers = await httpClient.GetFromJsonAsync<List<ArcaneVaultUser>>("ArcaneVaultUser");

            if (existingUsers != null)
            {
                var duplicateEmail = existingUsers.Any(u =>
                    u.Email.Equals(Input.Email, StringComparison.OrdinalIgnoreCase));

                if (duplicateEmail)
                {
                    ModelState.AddModelError(string.Empty, "Email has already been registered.");
                    return Page();
                }

                var duplicateUsername = existingUsers.Any(u =>
                    u.UserName.Equals(Input.Username, StringComparison.OrdinalIgnoreCase));

                if (duplicateUsername)
                {
                    ModelState.AddModelError(string.Empty, "Username has already been taken.");
                    return Page();
                }
            }

            var newUser = new ArcaneVaultUserDTO
            {
                UserName = Input.Username,
                Email = Input.Email,
                IsDeleted = false,
                RoleId = 2,
                Password = Input.Password
            };

            var response = await httpClient.PostAsJsonAsync("ArcaneVaultUser", newUser);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while registering. Please try again.");
                return Page();
            }
        }
    }
}