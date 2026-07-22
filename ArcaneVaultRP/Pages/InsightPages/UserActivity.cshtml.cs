using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.InsightPages;

public class UserActivityModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UserActivityModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public UserActivityDetail ActivityDetail { get; set; } = new();
    public int DAU { get; set; }
    public int WAU { get; set; }
    public int MAU { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Days { get; set; } = 30;

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");
        if (HttpContext.Session.GetString(SessionKeys.UserRole) != "1")
            return RedirectToPage("/Index");

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // DAU / WAU / MAU
        var dauRes = await httpClient.GetFromJsonAsync<CountResult>("LoginLog/dau");
        var wauRes = await httpClient.GetFromJsonAsync<CountResult>("LoginLog/wau");
        var mauRes = await httpClient.GetFromJsonAsync<CountResult>("LoginLog/mau");
        DAU = dauRes?.Count ?? 0;
        WAU = wauRes?.Count ?? 0;
        MAU = mauRes?.Count ?? 0;

        // Detail data
        ActivityDetail = await httpClient
            .GetFromJsonAsync<UserActivityDetail>($"Insights/useractivity/detail?days={Days}")
            ?? new UserActivityDetail();

        return Page();
    }
}