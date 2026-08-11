using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.CategoryPages;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EditModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public Category Category { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string? categorycode)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");
        if (HttpContext.Session.GetString(SessionKeys.UserRole) != "1")
            return RedirectToPage("/Index");
        if (categorycode is null)
            return NotFound();

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        var allCategories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                            ?? new List<Category>();

        // LINQ — find category
        var category = allCategories.FirstOrDefault(c => c.CategoryCode == categorycode);
        if (category is null)
            return NotFound();

        Category = category;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var trimmedName = Category.CategoryName?.Trim() ?? string.Empty;

        // Minimum length check
        if (trimmedName.Length < 3)
        {
            ModelState.AddModelError(string.Empty, "Category name must be at least 3 characters long.");
            return Page();
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        var allCategories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                            ?? new List<Category>();

        // LINQ — exclude current category from checks
        var otherCategories = allCategories
            .Where(c => c.CategoryCode != Category.CategoryCode)
            .ToList();

        // LINQ — duplicate name check (both sides trimmed)
        var duplicateName = otherCategories.Any(c =>
            c.CategoryName.Trim().Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

        if (duplicateName)
        {
            ModelState.AddModelError(string.Empty, "A category with this name already exists.");
            return Page();
        }

        string oldCode = Category.CategoryCode;

        // Generate new code — excluding old code so its slot is freed
        try
        {
            var existingCodes = otherCategories.Select(c => c.CategoryCode).ToList();
            string newCode = CategoryCodeHelper.GenerateCategoryCode(trimmedName, existingCodes);
            Category.CategoryName = trimmedName;

            // Delete old category
            await httpClient.DeleteAsync($"Category/{oldCode}");

            // Create new category with new code
            var updatedCategory = new Category
            {
                CategoryCode = newCode,
                CategoryName = trimmedName
            };

            var response = await httpClient.PostAsJsonAsync("Category", updatedCategory);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("./Index", new { updated = true });

            ModelState.AddModelError(string.Empty, "An error occurred while updating the category.");
            return Page();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}