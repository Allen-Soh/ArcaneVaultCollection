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
        {
            return RedirectToPage("/Login");
        }
        if (HttpContext.Session.GetString(SessionKeys.UserRole) != "1")
        {
            return RedirectToPage("/Index");
        }

        if (categorycode is null)
        {
            return NotFound();
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // LINQ — find category by code
        var allCategories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                            ?? new List<Category>();

        var category = allCategories
            .FirstOrDefault(c => c.CategoryCode == categorycode);

        if (category is null)
        {
            return NotFound();
        }

        Category = category;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Get all categories except current one
        var allCategories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                            ?? new List<Category>();

        // LINQ — exclude current category from duplicate check
        var otherCategories = allCategories
            .Where(c => c.CategoryCode != Category.CategoryCode)
            .ToList();

        // LINQ — check duplicate name
        var duplicateName = otherCategories.Any(c =>
            c.CategoryName.Equals(Category.CategoryName, StringComparison.OrdinalIgnoreCase));

        if (duplicateName)
        {
            ModelState.AddModelError(string.Empty, "Category name already exists.");
            return Page();
        }

        // Generate new category code from updated name
        string oldCategoryCode = Category.CategoryCode;
        string newCategoryCode = GenerateCategoryCode(Category.CategoryName, otherCategories);

        // Delete old category first
        await httpClient.DeleteAsync($"Category/{oldCategoryCode}");

        // Create new category with new code
        var updatedCategory = new Category
        {
            CategoryCode = newCategoryCode,
            CategoryName = Category.CategoryName
        };

        var response = await httpClient.PostAsJsonAsync("Category", updatedCategory);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "An error occurred while updating the category.");
            return Page();
        }
    }

    private string GenerateCategoryCode(string categoryName, List<Category> existingCategories)
    {
        var code = new string(categoryName
            .ToUpper()
            .Where(char.IsLetterOrDigit)
            .Take(4)
            .ToArray());

        // LINQ — check if code already exists
        if (existingCategories.Any(c => c.CategoryCode == code))
        {
            int suffix = 1;
            string newCode;
            do
            {
                newCode = code.Substring(0, Math.Min(code.Length, 3)) + suffix.ToString();
                suffix++;
            }
            while (existingCategories.Any(c => c.CategoryCode == newCode));

            return newCode;
        }

        return code;
    }
}