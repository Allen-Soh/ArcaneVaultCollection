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
        {
            return RedirectToPage("/Login");
        }
        if (HttpContext.Session.GetString(SessionKeys.UserRole) != "1")
        {
            return RedirectToPage("/Index");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Category.CategoryCode");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Get existing categories
        var existingCategories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                                 ?? new List<Category>();

        // LINQ — check duplicate category name
        var duplicateName = existingCategories.Any(c =>
            c.CategoryName.Equals(Category.CategoryName, StringComparison.OrdinalIgnoreCase));

        if (duplicateName)
        {
            ModelState.AddModelError(string.Empty, "Category name already exists.");
            return Page();
        }

        // Generate category code from category name
        Category.CategoryCode = GenerateCategoryCode(Category.CategoryName, existingCategories);

        var response = await httpClient.PostAsJsonAsync("Category", Category);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "An error occurred while creating the category.");
            return Page();
        }
    }

    private string GenerateCategoryCode(string categoryName, List<Category> existingCategories)
    {
        // Remove spaces and special characters, take first 4 alphanumeric characters uppercase
        var code = new string(categoryName
            .ToUpper()
            .Where(char.IsLetterOrDigit)
            .Take(4)
            .ToArray());

        // LINQ — check if code already exists, append number if it does
        if (existingCategories.Any(c => c.CategoryCode == code))
        {
            int suffix = 1;
            string newCode;
            do
            {
                // Trim code to make room for suffix if needed
                newCode = code.Substring(0, Math.Min(code.Length, 3)) + suffix.ToString();
                suffix++;
            }
            while (existingCategories.Any(c => c.CategoryCode == newCode));

            return newCode;
        }

        return code;
    }
}