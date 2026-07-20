using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.InsightPages;

public class PopularItemsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PopularItemsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<PopularItemDetail> Items { get; set; } = new();
    public List<string> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        if (HttpContext.Session.GetString(SessionKeys.UserRole) != "1")
            return RedirectToPage("/Index");

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        Items = await httpClient
            .GetFromJsonAsync<List<PopularItemDetail>>("Insights/popularitems/detail")
            ?? new List<PopularItemDetail>();

        // LINQ — extract unique sorted category list for dropdown
        Categories = Items
            .Select(i => i.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        return Page();
    }
}