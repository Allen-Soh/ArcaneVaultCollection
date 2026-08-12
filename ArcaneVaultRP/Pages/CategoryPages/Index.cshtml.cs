using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.CategoryPages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const int PageSize = 10;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<Category> AllCategories { get; set; } = new(); // full list — powers the name-suggestion dropdown
    public List<Category> PagedCategories { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
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

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        await LoadCategoriesAsync(httpClient, null, 1);

        return Page();
    }

    public async Task<IActionResult> OnGetFilterAsync(
        string? searchCategoryName = null,
        [FromQuery(Name = "page")] int pageNumber = 1)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        await LoadCategoriesAsync(httpClient, searchCategoryName, pageNumber);

        return Partial("_CategoryTable", this);
    }

    private async Task LoadCategoriesAsync(HttpClient httpClient, string? searchCategoryName, int pageNumber)
    {
        var categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                         ?? new List<Category>();

        AllCategories = categories.OrderBy(c => c.CategoryName).ToList();
        SearchCategoryName = searchCategoryName;

        var filtered = AllCategories.AsEnumerable();

        if (!string.IsNullOrEmpty(searchCategoryName))
        {
            filtered = filtered.Where(c =>
                (c.CategoryName != null && c.CategoryName.Contains(searchCategoryName, StringComparison.OrdinalIgnoreCase)) ||
                (c.CategoryCode != null && c.CategoryCode.Contains(searchCategoryName, StringComparison.OrdinalIgnoreCase)));
        }

        var filteredList = filtered.ToList();

        TotalPages = Math.Max(1, (int)Math.Ceiling(filteredList.Count / (double)PageSize));
        CurrentPage = Math.Clamp(pageNumber < 1 ? 1 : pageNumber, 1, TotalPages);

        PagedCategories = filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    // Helper — truncate text to 40 chars with ...
    public string Truncate(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= 40 ? text : text.Substring(0, 40) + "...";
    }
}
