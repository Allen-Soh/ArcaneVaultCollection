//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.CollectionItemPages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const int PageSize = 10;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<CollectionItemCategory> AllItems { get; set; } = new(); // full list — powers the name/category suggestion dropdowns
    public List<Category> Categories { get; set; } = new();
    public List<CollectionItemCategory> PagedItems { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public string? SearchItemName { get; set; }
    public string? SearchCategory { get; set; }
    public int? SearchCurrentQuantity { get; set; }
    public bool IsStaff { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
        {
            return RedirectToPage("/Login");
        }

        IsStaff = HttpContext.Session.GetString(SessionKeys.UserRole) == "1";

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        await LoadItemsAsync(httpClient, loggedInUser.UserName, null, null, null, 1);

        return Page();
    }

    public async Task<IActionResult> OnGetFilterAsync(
        string? searchItemName = null,
        string? searchCategory = null,
        int? searchCurrentQuantity = null,
        [FromQuery(Name = "page")] int pageNumber = 1)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        IsStaff = HttpContext.Session.GetString(SessionKeys.UserRole) == "1";

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        await LoadItemsAsync(httpClient, loggedInUser.UserName, searchItemName, searchCategory, searchCurrentQuantity, pageNumber);

        return Partial("_CollectionItemTable", this);
    }

    private async Task LoadItemsAsync(
        HttpClient httpClient,
        string userName,
        string? searchItemName,
        string? searchCategory,
        int? searchCurrentQuantity,
        int pageNumber)
    {
        var collectionItems = await httpClient
            .GetFromJsonAsync<List<CollectionItemCategory>>("CollectionItem/withcategory")
            ?? new List<CollectionItemCategory>();

        Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                     ?? new List<Category>();

        AllItems = collectionItems
            .Where(c => c.UserName == userName)
            .OrderBy(c => c.ItemName)
            .ToList();

        SearchItemName = searchItemName;
        SearchCategory = searchCategory;
        SearchCurrentQuantity = searchCurrentQuantity;

        var filtered = AllItems.AsEnumerable();

        if (!string.IsNullOrEmpty(searchItemName))
            filtered = filtered.Where(i =>
                i.ItemName != null && i.ItemName.Contains(searchItemName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(searchCategory))
            filtered = filtered.Where(i =>
                i.CategoryName != null && i.CategoryName.Contains(searchCategory, StringComparison.OrdinalIgnoreCase));

        if (searchCurrentQuantity.HasValue)
            filtered = filtered.Where(i => i.CurrentQuantity == searchCurrentQuantity.Value);

        var filteredList = filtered.ToList();

        TotalPages = Math.Max(1, (int)Math.Ceiling(filteredList.Count / (double)PageSize));
        CurrentPage = Math.Clamp(pageNumber < 1 ? 1 : pageNumber, 1, TotalPages);

        PagedItems = filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    // Helper — truncate text to 40 chars with ...
    public string Truncate(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= 40 ? text : text.Substring(0, 40) + "...";
    }
}
