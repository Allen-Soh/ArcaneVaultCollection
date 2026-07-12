using ArcaneVaultAPI.DataTransferObject;
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
        {
            return RedirectToPage("/Login");
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Load categories for dropdown
        Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                     ?? new List<Category>();

        // LINQ — sort categories by name
        Categories = Categories.OrderBy(c => c.CategoryName).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("CollectionItem.UserName");

        if (!ModelState.IsValid)
        {
            // Reload categories if validation fails
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

        // Step 1 — POST CollectionItem
        var dto = new CollectionItemDTO
        {
            ItemName = CollectionItem.ItemName,
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

        // Step 2 — get new ItemId from response
        var createdItem = await response.Content.ReadFromJsonAsync<CollectionItem>();

        // Step 3 — POST CollectionItemCategory if category was selected
        if (!string.IsNullOrEmpty(SelectedCategoryCode) && createdItem != null)
        {
            // Delete any existing link first to prevent UNIQUE constraint error
            await httpClient.DeleteAsync($"CollectionItemCategory/{createdItem.ItemId}/{SelectedCategoryCode}");

            var categoryLink = new CollectionItemCategory
            {
                ItemId = createdItem.ItemId,
                CategoryCode = SelectedCategoryCode
            };

            var categoryResponse = await httpClient.PostAsJsonAsync("CollectionItemCategory", categoryLink);

            if (!categoryResponse.IsSuccessStatusCode)
            {
                var error = await categoryResponse.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Category link error: {error}");
                return Page();
            }
        }

        return RedirectToPage("./Index");
    }
}