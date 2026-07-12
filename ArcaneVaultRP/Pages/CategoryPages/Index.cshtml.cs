using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.CategoryPages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IList<Category> Category { get; set; } = default!;

    // Single search field for both name and code
    [BindProperty(SupportsGet = true)]
    public string? SearchCategoryName { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
        {
            return RedirectToPage("/Login");
        }

        if (HttpContext.Session.GetString(SessionKeys.UserRole) != "1")
        {
            return RedirectToPage("/Index");
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                         ?? new List<Category>();

        // LINQ — filter by category name OR code in one search
        if (!string.IsNullOrEmpty(SearchCategoryName))
        {
            SearchCategoryName = SearchCategoryName.Trim();
            if (SearchCategoryName.Length > 100)
                SearchCategoryName = SearchCategoryName.Substring(0, 100);

            categories = categories
                .Where(c =>
                    (c.CategoryName != null && c.CategoryName.Contains(SearchCategoryName, StringComparison.OrdinalIgnoreCase)) ||
                    (c.CategoryCode != null && c.CategoryCode.Contains(SearchCategoryName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // LINQ — sort by CategoryName
        Category = categories
            .OrderBy(c => c.CategoryName)
            .ToList();

        return Page();
    }
}