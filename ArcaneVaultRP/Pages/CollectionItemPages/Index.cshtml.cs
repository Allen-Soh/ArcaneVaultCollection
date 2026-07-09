using ArcaneVaultAPI.Data;
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

    public IList<CollectionItem> CollectionItem { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? SearchItemName { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SearchStartingQuantity { get; set; }

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
        var collectionItems = await httpClient.GetFromJsonAsync<List<CollectionItem>>("CollectionItem") ?? new List<CollectionItem>();

        collectionItems = collectionItems
            .Where(c => c.UserName == loggedInUser.UserName)
            .ToList();

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

        if (SearchStartingQuantity.HasValue)
        {
            collectionItems = collectionItems.Where(c => c.StartingQuantity == SearchStartingQuantity.Value).ToList();
        }

        if (SearchCurrentQuantity.HasValue)
        {
            collectionItems = collectionItems.Where(c => c.CurrentQuantity == SearchCurrentQuantity.Value).ToList();
        }

        CollectionItem = collectionItems;
        return Page();
    }
}