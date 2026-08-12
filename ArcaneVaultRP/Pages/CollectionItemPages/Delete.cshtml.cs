//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.CollectionItemPages;

public class DeleteModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DeleteModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public CollectionItem CollectionItem { get; set; } = default!;
    public Category? ItemCategory { get; set; }

    public async Task<IActionResult> OnGetAsync(int? itemid)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
        {
            return RedirectToPage("/Login");
        }

        if (itemid is null)
        {
            return NotFound();
        }

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        // LINQ — find item and verify ownership
        var allItems = await httpClient.GetFromJsonAsync<List<CollectionItem>>("CollectionItem")
                       ?? new List<CollectionItem>();

        var collectionitem = allItems
            .FirstOrDefault(c => c.ItemId == itemid && c.UserName == loggedInUser.UserName);

        if (collectionitem is null)
        {
            return NotFound();
        }

        CollectionItem = collectionitem;

        // Get category for this item
        var allItemCategories = await httpClient
            .GetFromJsonAsync<List<CollectionItemCategory>>("CollectionItemCategory")
            ?? new List<CollectionItemCategory>();

        // LINQ — find category link for this item
        var itemCategory = allItemCategories
            .FirstOrDefault(c => c.ItemId == itemid);

        if (itemCategory != null)
        {
            var allCategories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                                ?? new List<Category>();

            // LINQ — find category by code
            ItemCategory = allCategories
                .FirstOrDefault(c => c.CategoryCode == itemCategory.CategoryCode);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? itemid)
    {
        if (itemid is null)
        {
            return NotFound();
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        // Step 1 — Delete CollectionItemCategory link first
        var allItemCategories = await httpClient
            .GetFromJsonAsync<List<CollectionItemCategory>>("CollectionItemCategory")
            ?? new List<CollectionItemCategory>();

        // LINQ — find category link for this item
        var itemCategory = allItemCategories
            .FirstOrDefault(c => c.ItemId == itemid);

        if (itemCategory != null)
        {
            await httpClient.DeleteAsync($"CollectionItemCategory/{itemid}/{itemCategory.CategoryCode}");
        }

        // Step 2 — Delete CollectionItem
        var response = await httpClient.DeleteAsync($"CollectionItem/{itemid}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "An error occurred while deleting the collection item.");
            return Page();
        }
    }
}