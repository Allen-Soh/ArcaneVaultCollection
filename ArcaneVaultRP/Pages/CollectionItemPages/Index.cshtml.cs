using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
    public string? SearchString { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
        {
            return RedirectToPage("/Login");
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var collectionItems = await httpClient.GetFromJsonAsync<List<CollectionItem>>("CollectionItem") ?? new List<CollectionItem>();

        if (!string.IsNullOrEmpty(SearchString))
        {
            SearchString = SearchString.Trim();

            if (SearchString.Length > 100)
            {
                SearchString = SearchString.Substring(0, 100);
            }
        }

        if (!string.IsNullOrEmpty(SearchString))
        {
            collectionItems = collectionItems.Where(c =>
                (c.ItemName != null && c.ItemName.Contains(SearchString, StringComparison.OrdinalIgnoreCase)) ||
                (c.UserName != null && c.UserName.Contains(SearchString, StringComparison.OrdinalIgnoreCase)) ||
                c.ItemId.ToString().Contains(SearchString) ||
                c.StartingQuantity.ToString().Contains(SearchString) ||
                c.EndingQuantity.ToString().Contains(SearchString)
            ).ToList();
        }

        CollectionItem = collectionItems;
        return Page();
    }
}