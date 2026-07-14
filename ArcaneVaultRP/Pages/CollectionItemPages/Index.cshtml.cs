using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.CollectionItemPages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IList<CollectionItemCategory> CollectionItem { get; set; } = default!;
    public IList<Category> Categories { get; set; } = default!;
    public IList<CollectionItemCategory> AllItems { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? SearchItemName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchCategory { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SearchCurrentQuantity { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
        {
            return RedirectToPage("/Login");
        }

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Get all items with category info
        var collectionItems = await httpClient
            .GetFromJsonAsync<List<CollectionItemCategory>>("CollectionItem/withcategory")
            ?? new List<CollectionItemCategory>();

        // Get all categories for dropdown
        Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                     ?? new List<Category>();

        // LINQ — filter by logged in user
        collectionItems = collectionItems
            .Where(c => c.UserName == loggedInUser.UserName)
            .ToList();

        AllItems = collectionItems.OrderBy(c => c.ItemName).ToList();

        // LINQ — smart search by Item Name
        if (!string.IsNullOrEmpty(SearchItemName))
        {
            SearchItemName = SearchItemName.Trim();
            if (SearchItemName.Length > 100)
                SearchItemName = SearchItemName.Substring(0, 100);

            collectionItems = collectionItems
                .Where(c => c.ItemName != null &&
                       c.ItemName.Contains(SearchItemName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // LINQ — smart search by Category (name)
        if (!string.IsNullOrEmpty(SearchCategory))
        {
            SearchCategory = SearchCategory.Trim();

            collectionItems = collectionItems
                .Where(c =>
                    (c.CategoryName != null && c.CategoryName.Contains(SearchCategory, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // LINQ — filter by Current Quantity
        if (SearchCurrentQuantity.HasValue)
        {
            collectionItems = collectionItems
                .Where(c => c.CurrentQuantity == SearchCurrentQuantity.Value)
                .ToList();
        }

        // LINQ — sort by ItemName ascending
        collectionItems = collectionItems
            .OrderBy(c => c.ItemName)
            .ToList();

        CollectionItem = collectionItems;
        return Page();
    }

    // Helper — truncate text to 15 chars with ...
    public string Truncate(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= 40 ? text : text.Substring(0, 40) + "...";
    }
}