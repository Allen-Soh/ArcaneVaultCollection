using ArcaneVaultAPI.DataTransferObject;
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

    [BindProperty]
    public string? ImageUrl { get; set; }

    [BindProperty]
    public string? ImagePublicId { get; set; }

    [BindProperty]
    public string? DeletePublicId { get; set; }

    public IList<Category> Categories { get; set; } = default!;
    public string? CurrentCategoryCode { get; set; }
    public string? CurrentCategoryName { get; set; }

    public async Task<IActionResult> OnGetAsync(int? itemid)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");
        if (itemid is null) return NotFound();

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        var allItems = await httpClient
            .GetFromJsonAsync<List<CollectionItem>>("CollectionItem")
            ?? new List<CollectionItem>();
        var collectionitem = allItems.FirstOrDefault(c => c.ItemId == itemid);
        if (collectionitem is null) return NotFound();

        CollectionItem = collectionitem;
        ImageUrl = collectionitem.ImageUrl;
        ImagePublicId = collectionitem.ImagePublicId;

        Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                     ?? new List<Category>();
        Categories = Categories.OrderBy(c => c.CategoryName).ToList();

        var allItemCategories = await httpClient
            .GetFromJsonAsync<List<CollectionItemCategory>>("CollectionItemCategory")
            ?? new List<CollectionItemCategory>();
        var existingLink = allItemCategories.FirstOrDefault(c => c.ItemId == itemid);
        if (existingLink != null)
        {
            CurrentCategoryCode = existingLink.CategoryCode;
            var cat = Categories.FirstOrDefault(c => c.CategoryCode == CurrentCategoryCode);
            CurrentCategoryName = cat?.CategoryName;
        }

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

        // Delete old image from Cloudinary if marked for deletion
        if (!string.IsNullOrEmpty(DeletePublicId))
        {
            await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete,
                $"{httpClient.BaseAddress}ImageUpload")
            {
                Content = JsonContent.Create(DeletePublicId)
            });
        }

        var dto = new CollectionItemDTO
        {
            ItemId = CollectionItem.ItemId,
            ItemName = CollectionItem.ItemName,
            IsDeleted = CollectionItem.IsDeleted,
            StartingQuantity = CollectionItem.StartingQuantity,
            CurrentQuantity = CollectionItem.CurrentQuantity,
            UserName = loggedInUser.UserName,
            ImageUrl = string.IsNullOrEmpty(ImageUrl) ? null : ImageUrl,
            ImagePublicId = string.IsNullOrEmpty(ImagePublicId) ? null : ImagePublicId
        };

        var response = await httpClient
            .PutAsJsonAsync($"CollectionItem/{CollectionItem.ItemId}", dto);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Error {response.StatusCode}: {error}");
            Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                         ?? new List<Category>();
            return Page();
        }

        // Update category link
        var allItemCategories = await httpClient
            .GetFromJsonAsync<List<CollectionItemCategory>>("CollectionItemCategory")
            ?? new List<CollectionItemCategory>();
        var existingLink = allItemCategories
            .FirstOrDefault(c => c.ItemId == CollectionItem.ItemId);
        if (existingLink != null)
            await httpClient.DeleteAsync(
                $"CollectionItemCategory/{CollectionItem.ItemId}/{existingLink.CategoryCode}");
        if (!string.IsNullOrEmpty(SelectedCategoryCode))
            await httpClient.PostAsJsonAsync("CollectionItemCategory",
                new CollectionItemCategory
                {
                    ItemId = CollectionItem.ItemId,
                    CategoryCode = SelectedCategoryCode
                });

        return RedirectToPage("./Index", new { updated = true });
    }
}