using ArcaneVaultRP.Helpers;
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

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
        {
            return RedirectToPage("/Login");
        }

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

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

        // Filtering now happens client-side (live, as-you-type) — see applyFilters() in the view.
        CollectionItem = AllItems;
        return Page();
    }

    // Helper — truncate text to 15 chars with ...
    public string Truncate(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= 40 ? text : text.Substring(0, 40) + "...";
    }
}