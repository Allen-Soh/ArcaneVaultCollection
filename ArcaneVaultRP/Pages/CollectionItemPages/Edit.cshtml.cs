using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.CollectionItemPages;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EditModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public CollectionItem CollectionItem { get; set; } = default!;

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

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var collectionitem = await httpClient.GetFromJsonAsync<CollectionItem>($"CollectionItem/{itemid}");

        if (collectionitem is null)
        {
            return NotFound();
        }

        CollectionItem = collectionitem;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("CollectionItem_UserName");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Get logged in user from session
        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultAPI.Data.ArcaneVaultUser>(userJson);

        // Build DTO — only update ItemName and CurrentQuantity
        var dto = new CollectionItem
        {
            ItemId = CollectionItem.ItemId,
            ItemName = CollectionItem.ItemName,
            IsDeleted = CollectionItem.IsDeleted,
            StartingQuantity = CollectionItem.StartingQuantity,
            CurrentQuantity = CollectionItem.CurrentQuantity,
            UserName = CollectionItem.UserName,
        };

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var response = await httpClient.PutAsJsonAsync($"CollectionItem/{CollectionItem.ItemId}", dto);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("./Index");
        }
        else
        {
            //ModelState.AddModelError(string.Empty, "An error occurred while updating the collection item.");
            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error {response.StatusCode}: {errorContent}");
            return Page();
        }
    }
}