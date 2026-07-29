using ArcaneVaultAPI.DataTransferObject;
using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.CollectionItemPages;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IList<Category> Categories { get; set; } = default!;

    [BindProperty]
    public CollectionItem CollectionItem { get; set; } = default!;

    [BindProperty]
    public string? SelectedCategoryCode { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                     ?? new List<Category>();

        // LINQ — sort categories
        Categories = Categories.OrderBy(c => c.CategoryName).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("CollectionItem.UserName");

        if (!ModelState.IsValid)
        {
            HttpClient httpClientReload = _httpClientFactory.CreateClient("ArcaneVaultAPI");

            Categories = await httpClientReload.GetFromJsonAsync<List<Category>>("Category")
                         ?? new List<Category>();
            return Page();
        }

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        if (loggedInUser == null)
            return RedirectToPage("/Login");

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        // Get all active items for logged in user
        var allItems = await httpClient.GetFromJsonAsync<List<CollectionItem>>("CollectionItem")
                       ?? new List<CollectionItem>();

        // LINQ — duplicate item name check (both sides trimmed, active only)
        var trimmedName = CollectionItem.ItemName?.Trim() ?? string.Empty;
        var duplicateItem = allItems.Any(c =>
            c.UserName == loggedInUser.UserName &&
            !c.IsDeleted &&
            c.ItemName.Trim().Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

        if (duplicateItem)
        {
            ModelState.AddModelError(string.Empty, "An item with this name already exists in your collection.");
            Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                         ?? new List<Category>();
            return Page();
        }

        // Step 1 — POST CollectionItem
        var dto = new CollectionItemDTO
        {
            ItemName = trimmedName,
            IsDeleted = false,
            StartingQuantity = CollectionItem.StartingQuantity,
            CurrentQuantity = CollectionItem.StartingQuantity,
            UserName = loggedInUser.UserName
        };

        var response = await httpClient.PostAsJsonAsync("CollectionItem", dto);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while creating the collection item.");
            Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                         ?? new List<Category>();
            return Page();
        }

        // Step 2 — get new ItemId
        var createdItem = await response.Content.ReadFromJsonAsync<CollectionItem>();

        // Step 3 — POST CollectionItemCategory if selected
        if (!string.IsNullOrEmpty(SelectedCategoryCode) && createdItem != null)
        {
            await httpClient.DeleteAsync($"CollectionItemCategory/{createdItem.ItemId}/{SelectedCategoryCode}");

            var categoryLink = new CollectionItemCategory
            {
                ItemId = createdItem.ItemId,
                CategoryCode = SelectedCategoryCode
            };

            await httpClient.PostAsJsonAsync("CollectionItemCategory", categoryLink);
        }

        return RedirectToPage("./Index");
    }
}