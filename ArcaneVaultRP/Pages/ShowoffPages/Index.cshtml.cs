using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.Showoffs;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const int PageSize = 8;
    private const int CosmeticPageSize = 4;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string ActiveTab { get; set; } = "feed";
    public string SortBy { get; set; } = "popular";
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public string? SearchUser { get; set; }
    public string? SearchItem { get; set; }
    public string? SearchCategory { get; set; }

    public List<Showoff> PagedShowoffs { get; set; } = new();
    public List<Showoff> AllShowoffs { get; set; } = new();
    public List<Showoff> UserShowoffs { get; set; } = new();
    public List<CollectionItemCategory> UserCollectionItems { get; set; } = new();
    public List<Category> Categories { get; set; } = new();

    // Cosmetics — each section is paged independently, server-side, 4 per page.
    public List<Frame> PagedFrames { get; set; } = new();
    public int FrameCurrentPage { get; set; } = 1;
    public int FrameTotalPages { get; set; } = 1;

    public List<Sticker> PagedStickers { get; set; } = new();
    public int StickerCurrentPage { get; set; } = 1;
    public int StickerTotalPages { get; set; } = 1;

    // ASSUMPTION: ArcaneVaultUser has a RoleId property where 1 == Staff.
    // Swap this for whatever your actual staff/role check is if it differs.
    public bool IsStaff { get; set; }

    public async Task<IActionResult> OnGetAsync(
        string tab = "feed",
        string sort = "popular",
        [FromQuery(Name = "page")] int pageNumber = 1,
        string? searchUser = null,
        string? searchItem = null,
        string? searchCategory = null)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);
        IsStaff = loggedInUser?.RoleId == 1;

        ActiveTab = tab;

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        UserShowoffs = await httpClient
            .GetFromJsonAsync<List<Showoff>>($"Showoff/user/{loggedInUser.UserName}?requester={loggedInUser.UserName}")
            ?? new List<Showoff>();

        UserCollectionItems = await httpClient
            .GetFromJsonAsync<List<CollectionItemCategory>>("CollectionItem/withcategory")
            ?? new List<CollectionItemCategory>();

        UserCollectionItems = UserCollectionItems
            .Where(c => c.UserName == loggedInUser.UserName && !c.IsDeleted)
            .OrderBy(c => c.ItemName)
            .ToList();

        Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                     ?? new List<Category>();

        // Only bother fetching cosmetics when the Shop tab is actually being viewed —
        // avoids unnecessary API calls on every Feed/Profile page load.
        if (ActiveTab == "shop")
        {
            await LoadShopDataAsync(httpClient, 1, 1, 1);
        }

        await LoadFeedDataAsync(httpClient, sort, pageNumber, searchUser, searchItem, searchCategory);

        return Page();
    }

    public async Task<IActionResult> OnGetFilterAsync(
        string tab = "feed",
        string sort = "popular",
        [FromQuery(Name = "page")] int pageNumber = 1,
        string? searchUser = null,
        string? searchItem = null,
        string? searchCategory = null)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        ActiveTab = tab;

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        await LoadFeedDataAsync(httpClient, sort, pageNumber, searchUser, searchItem, searchCategory);

        return Partial("_ShowoffGrid", this);
    }

    // ── Cosmetic grid AJAX handlers — each fetches only its own list and pages it,
    // rather than the old approach of loading all three catalogs on every request. ──

    public async Task<IActionResult> OnGetFrameGridAsync([FromQuery(Name = "page")] int pageNumber = 1)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        SetIsStaffFromSession();

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        var allFrames = (await httpClient.GetFromJsonAsync<List<Frame>>("Frame") ?? new List<Frame>())
            .OrderBy(f => f.FrameName)
            .ToList();

        var (paged, totalPages, currentPage) = PageCosmetics(allFrames, pageNumber);
        PagedFrames = paged;
        FrameTotalPages = totalPages;
        FrameCurrentPage = currentPage;

        return Partial("_FrameGrid", this);
    }

    public async Task<IActionResult> OnGetStickerGridAsync([FromQuery(Name = "page")] int pageNumber = 1)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        SetIsStaffFromSession();

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        var allStickers = (await httpClient.GetFromJsonAsync<List<Sticker>>("Sticker") ?? new List<Sticker>())
            .OrderBy(s => s.StickerName)
            .ToList();

        var (paged, totalPages, currentPage) = PageCosmetics(allStickers, pageNumber);
        PagedStickers = paged;
        StickerTotalPages = totalPages;
        StickerCurrentPage = currentPage;

        return Partial("_StickerGrid", this);
    }

    private void SetIsStaffFromSession()
    {
        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        if (userJson == null) return;
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);
        IsStaff = loggedInUser?.RoleId == 1;
    }

    // Shared paging math for all three cosmetic types — keeps OnGetAsync's initial
    // (page 1) load and the three AJAX handlers in sync with identical clamp behavior.
    private static (List<T> Paged, int TotalPages, int CurrentPage) PageCosmetics<T>(List<T> all, int requestedPage)
    {
        var totalPages = Math.Max(1, (int)Math.Ceiling(all.Count / (double)CosmeticPageSize));
        var currentPage = Math.Clamp(requestedPage < 1 ? 1 : requestedPage, 1, totalPages);
        var paged = all
            .Skip((currentPage - 1) * CosmeticPageSize)
            .Take(CosmeticPageSize)
            .ToList();
        return (paged, totalPages, currentPage);
    }

    private async Task LoadShopDataAsync(HttpClient httpClient, int framePage, int titlePage, int stickerPage)
    {
        // ASSUMPTION: endpoint names "Frame" / "Title" / "Sticker" — update to match your
        // actual API controller routes if they differ.
        var allFrames = (await httpClient.GetFromJsonAsync<List<Frame>>("Frame") ?? new List<Frame>())
            .OrderBy(f => f.FrameName)
            .ToList();
        var (pagedFrames, frameTotalPages, frameCurrentPage) = PageCosmetics(allFrames, framePage);
        PagedFrames = pagedFrames;
        FrameTotalPages = frameTotalPages;
        FrameCurrentPage = frameCurrentPage;

        var allStickers = (await httpClient.GetFromJsonAsync<List<Sticker>>("Sticker") ?? new List<Sticker>())
            .OrderBy(s => s.StickerName)
            .ToList();
        var (pagedStickers, stickerTotalPages, stickerCurrentPage) = PageCosmetics(allStickers, stickerPage);
        PagedStickers = pagedStickers;
        StickerTotalPages = stickerTotalPages;
        StickerCurrentPage = stickerCurrentPage;
    }

    private async Task LoadFeedDataAsync(
        HttpClient httpClient,
        string sort,
        int pageNumber,
        string? searchUser,
        string? searchItem,
        string? searchCategory)
    {
        SortBy = string.IsNullOrEmpty(sort) ? "popular" : sort;
        CurrentPage = pageNumber < 1 ? 1 : pageNumber;
        SearchUser = searchUser;
        SearchItem = searchItem;
        SearchCategory = searchCategory;

        AllShowoffs = await httpClient.GetFromJsonAsync<List<Showoff>>("Showoff")
                      ?? new List<Showoff>();

        var filtered = AllShowoffs.AsEnumerable();

        if (!string.IsNullOrEmpty(searchUser))
            filtered = filtered.Where(s =>
                s.UserName.Contains(searchUser, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(searchItem))
            filtered = filtered.Where(s =>
                s.Items.Any(i => i.ItemName != null &&
                    i.ItemName.Contains(searchItem, StringComparison.OrdinalIgnoreCase)));

        if (!string.IsNullOrEmpty(searchCategory))
            filtered = filtered.Where(s =>
                s.Items.Any(i => i.CategoryName != null &&
                    i.CategoryName.Contains(searchCategory, StringComparison.OrdinalIgnoreCase)));

        filtered = sort switch
        {
            "newest" => filtered.OrderByDescending(s => s.CreatedAt),
            "least" => filtered.OrderBy(s => s.LikeCount).ThenByDescending(s => s.CreatedAt),
            _ => filtered.OrderByDescending(s => s.LikeCount).ThenByDescending(s => s.CreatedAt)
        };

        var filteredList = filtered.ToList();

        TotalPages = (int)Math.Ceiling(filteredList.Count / (double)PageSize);
        TotalPages = Math.Max(TotalPages, 1);
        CurrentPage = Math.Clamp(CurrentPage, 1, TotalPages);

        PagedShowoffs = filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }
}