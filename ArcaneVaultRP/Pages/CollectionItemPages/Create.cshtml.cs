//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

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

    [BindProperty]
    public string? ImageUrl { get; set; }

    [BindProperty]
    public string? ImagePublicId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                     ?? new List<Category>();
        Categories = Categories.OrderBy(c => c.CategoryName).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("CollectionItem.UserName");

        if (!ModelState.IsValid)
        {
            HttpClient reload = _httpClientFactory.CreateClient("ArcaneVaultAPI");
            Categories = await reload.GetFromJsonAsync<List<Category>>("Category")
                         ?? new List<Category>();
            return Page();
        }

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);
        if (loggedInUser == null) return RedirectToPage("/Login");

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Duplicate item name check
        var allItems = await httpClient.GetFromJsonAsync<List<CollectionItem>>("CollectionItem")
                       ?? new List<CollectionItem>();
        var trimmedName = CollectionItem.ItemName?.Trim() ?? string.Empty;
        var duplicate = allItems.Any(c =>
            c.UserName == loggedInUser.UserName &&
            !c.IsDeleted &&
            c.ItemName.Trim().Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            ModelState.AddModelError(string.Empty,
                "An item with this name already exists in your collection.");
            Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                         ?? new List<Category>();
            return Page();
        }

        var dto = new CollectionItem
        {
            ItemName = trimmedName,
            IsDeleted = false,
            StartingQuantity = CollectionItem.StartingQuantity,
            CurrentQuantity = CollectionItem.StartingQuantity,
            UserName = loggedInUser.UserName,
            ImageUrl = string.IsNullOrEmpty(ImageUrl) ? null : ImageUrl,
            ImagePublicId = string.IsNullOrEmpty(ImagePublicId) ? null : ImagePublicId
        };

        var response = await httpClient.PostAsJsonAsync("CollectionItem", dto);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty,
                "An error occurred while creating the collection item.");
            Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                         ?? new List<Category>();
            return Page();
        }

        var createdItem = await response.Content.ReadFromJsonAsync<CollectionItem>();

        // Link category if selected
        if (!string.IsNullOrEmpty(SelectedCategoryCode) && createdItem != null)
        {
            await httpClient.DeleteAsync(
                $"CollectionItemCategory/{createdItem.ItemId}/{SelectedCategoryCode}");
            await httpClient.PostAsJsonAsync("CollectionItemCategory",
                new CollectionItemCategory
                {
                    ItemId = createdItem.ItemId,
                    CategoryCode = SelectedCategoryCode
                });
        }

        return RedirectToPage("./Index", new { created = true });
    }
}