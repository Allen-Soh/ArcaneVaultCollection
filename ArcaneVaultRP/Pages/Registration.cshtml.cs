//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using ArcaneVaultRP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultRP.Pages
{
    public class RegistrationModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRecaptchaService _recaptchaService;
        private readonly IConfiguration _configuration;

        public RegistrationModel(
            IHttpClientFactory httpClientFactory,
            IRecaptchaService recaptchaService,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _recaptchaService = recaptchaService;
            _configuration = configuration;
        }

        public string ReCaptchaSiteKey => _configuration["ReCaptcha:SiteKey"] ?? "";

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();

        public class RegisterInputModel
        {
            [Required(ErrorMessage = "Username is required.")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please select your country.")]
            public string Country { get; set; } = string.Empty;

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

            // ── reCAPTCHA verification — must pass before any API call happens ──
            var captchaToken = Request.Form["g-recaptcha-response"].ToString();
            var captchaOk = await _recaptchaService.VerifyAsync(captchaToken);
            if (!captchaOk)
            {
                ModelState.AddModelError(string.Empty, "Please complete the reCAPTCHA verification.");
                return Page();
            }

            HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

            // Attach JWT token from session
            var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
            HttpClientHelper.AttachJwtToken(httpClient, token);

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
                Password = Input.Password,
                Country = Input.Country
            };

            var response = await httpClient.PostAsJsonAsync("ArcaneVaultUser", newUser);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Login", new { registered = true });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while registering. Please try again.");
                return Page();
            }
        }
    }
}