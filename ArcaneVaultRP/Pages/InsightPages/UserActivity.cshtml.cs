//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
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
    public List<CountryDemographic> CountryData { get; set; } = new();
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

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        var dauRes = await httpClient.GetFromJsonAsync<CountResult>("LoginLog/dau");
        var wauRes = await httpClient.GetFromJsonAsync<CountResult>("LoginLog/wau");
        var mauRes = await httpClient.GetFromJsonAsync<CountResult>("LoginLog/mau");
        DAU = dauRes?.Count ?? 0;
        WAU = wauRes?.Count ?? 0;
        MAU = mauRes?.Count ?? 0;

        ActivityDetail = await httpClient
            .GetFromJsonAsync<UserActivityDetail>($"Insights/useractivity/detail?days={Days}")
            ?? new UserActivityDetail();

        // Country demographic
        CountryData = await httpClient
            .GetFromJsonAsync<List<CountryDemographic>>("Insights/countrydemographic")
            ?? new List<CountryDemographic>();

        return Page();
    }
}