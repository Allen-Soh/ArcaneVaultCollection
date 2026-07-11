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

    [BindProperty(SupportsGet = true)]
    public string? SearchCategoryName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchCategoryCode { get; set; }

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
        Category = await httpClient.GetFromJsonAsync<List<Category>>("Category") ?? new List<Category>();

        if (!string.IsNullOrEmpty(SearchCategoryName))
        {
            SearchCategoryName = SearchCategoryName.Trim();
            if (SearchCategoryName.Length > 100)
                SearchCategoryName = SearchCategoryName.Substring(0, 100);

            Category = Category
                .Where(c => c.CategoryName != null &&
                       c.CategoryName.Contains(SearchCategoryName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // LINQ — sort by CategoryName
        Category= Category
            .OrderBy(c => c.CategoryName)
            .ToList();

        if (!string.IsNullOrEmpty(SearchCategoryCode))
        {
            SearchCategoryCode = SearchCategoryCode.Trim();
            if (SearchCategoryCode.Length > 3)
                SearchCategoryCode = SearchCategoryCode.Substring(0, 3);

            Category = Category
                .Where(c => c.CategoryCode != null &&
                       c.CategoryCode.Contains(SearchCategoryCode, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        Category = Category;
        return Page();
    }
}
