using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.CategoryPages;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public Category Category { get; set; } = default!;

    public IActionResult OnGet()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");
        if (HttpContext.Session.GetString(SessionKeys.UserRole) != "1")
            return RedirectToPage("/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Category.CategoryCode");

        if (!ModelState.IsValid)
            return Page();

        // Trim input
        var trimmedName = Category.CategoryName?.Trim() ?? string.Empty;

        // Minimum length check on full name
        if (trimmedName.Length < 3)
        {
            ModelState.AddModelError(string.Empty, "Category name must be at least 3 characters long.");
            return Page();
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        var existingCategories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                                 ?? new List<Category>();

        // LINQ — duplicate name check (both sides trimmed)
        var duplicateName = existingCategories.Any(c =>
            c.CategoryName.Trim().Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

        if (duplicateName)
        {
            ModelState.AddModelError(string.Empty, "A category with this name already exists.");
            return Page();
        }

        // Generate category code
        try
        {
            var existingCodes = existingCategories.Select(c => c.CategoryCode).ToList();
            Category.CategoryCode = CategoryCodeHelper.GenerateCategoryCode(trimmedName, existingCodes);
            Category.CategoryName = trimmedName;
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        var response = await httpClient.PostAsJsonAsync("Category", Category);

        if (response.IsSuccessStatusCode)
            return RedirectToPage("./Index", new { created = true });

        ModelState.AddModelError(string.Empty, "An error occurred while creating the category.");
        return Page();
    }
}