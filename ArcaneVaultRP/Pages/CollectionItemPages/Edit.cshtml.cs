using ArcaneVaultAPI.DataTransferObject;
using ArcaneVaultRP.Helpers;
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

    [BindProperty]
    public string? SelectedCategoryCode { get; set; }

    public IList<Category> Categories { get; set; } = default!;
    public string? CurrentCategoryCode { get; set; }
    public string? CurrentCategoryName { get; set; }

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

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        // LINQ — find item by id
        var allItems = await httpClient.GetFromJsonAsync<List<CollectionItem>>("CollectionItem")
                       ?? new List<CollectionItem>();

        var collectionitem = allItems.FirstOrDefault(c => c.ItemId == itemid);

        if (collectionitem is null)
        {
            return NotFound();
        }

        CollectionItem = collectionitem;

        // Load categories
        Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                     ?? new List<Category>();

        // LINQ — sort categories by name
        Categories = Categories.OrderBy(c => c.CategoryName).ToList();

        // Get current category for this item
        var allItemCategories = await httpClient
            .GetFromJsonAsync<List<CollectionItemCategory>>("CollectionItemCategory")
            ?? new List<CollectionItemCategory>();

        // LINQ — find existing category link
        var itemCategory = allItemCategories.FirstOrDefault(c => c.ItemId == itemid);

        if (itemCategory != null)
        {
            CurrentCategoryCode = itemCategory.CategoryCode;

            // LINQ — get category name
            var category = Categories.FirstOrDefault(c => c.CategoryCode == CurrentCategoryCode);
            CurrentCategoryName = category?.CategoryName;
        }

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
        {
            return RedirectToPage("/Login");
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        // Update CollectionItem
        var dto = new CollectionItemDTO
        {
            ItemId = CollectionItem.ItemId,
            ItemName = CollectionItem.ItemName,
            IsDeleted = CollectionItem.IsDeleted,
            StartingQuantity = CollectionItem.StartingQuantity,
            CurrentQuantity = CollectionItem.CurrentQuantity,
            UserName = loggedInUser.UserName
        };

        var response = await httpClient.PutAsJsonAsync($"CollectionItem/{CollectionItem.ItemId}", dto);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error {response.StatusCode}: {errorContent}");
            Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                         ?? new List<Category>();
            return Page();
        }

        // Update category link
        var allItemCategories = await httpClient
            .GetFromJsonAsync<List<CollectionItemCategory>>("CollectionItemCategory")
            ?? new List<CollectionItemCategory>();

        // LINQ — find existing category link
        var existingLink = allItemCategories.FirstOrDefault(c => c.ItemId == CollectionItem.ItemId);

        if (existingLink != null)
        {
            // Delete old category link
            await httpClient.DeleteAsync($"CollectionItemCategory/{CollectionItem.ItemId}/{existingLink.CategoryCode}");
        }

        // Add new category link if selected
        if (!string.IsNullOrEmpty(SelectedCategoryCode))
        {
            var categoryLink = new CollectionItemCategory
            {
                ItemId = CollectionItem.ItemId,
                CategoryCode = SelectedCategoryCode
            };
            await httpClient.PostAsJsonAsync("CollectionItemCategory", categoryLink);
        }

        return RedirectToPage("./Index");
    }
}