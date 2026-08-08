using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.InsightPages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // Popular Items
    public List<PopularItem> PopularItems { get; set; } = new();
    public List<CategoryPopularity> CategoryBreakdown { get; set; } = new();

    // Market Demand
    public List<MarketDemandItem> MarketDemand { get; set; } = new();
    public List<ActivityPoint> MarketDemandOverTime { get; set; } = new();

    // User Activity
    public List<UserActivity> UserActivities { get; set; } = new();
    public List<ActivityPoint> LoginActivity { get; set; } = new();
    public int DAU { get; set; }
    public int WAU { get; set; }
    public int MAU { get; set; }

    // Community Interest
    public CommunityInterestData CommunityInterest { get; set; } = new();
    public List<SearchTrend> SearchTrends { get; set; } = new();
    public List<TimeSeriesPoint> CommunityOverTime { get; set; } = new();

    // Finance and Sales
    public FinanceAndSalesDetail FinanceAndSales { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int MarketDays { get; set; } = 7;

    [BindProperty(SupportsGet = true)]
    public int CommunityDays { get; set; } = 30;

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

        // Popular Items
        PopularItems = await httpClient
            .GetFromJsonAsync<List<PopularItem>>("Insights/popularitems")
            ?? new List<PopularItem>();

        CategoryBreakdown = await httpClient
            .GetFromJsonAsync<List<CategoryPopularity>>("Insights/popularitems/categories")
            ?? new List<CategoryPopularity>();

        // Market Demand
        MarketDemand = await httpClient
            .GetFromJsonAsync<List<MarketDemandItem>>($"Insights/marketdemand?days={MarketDays}")
            ?? new List<MarketDemandItem>();

        MarketDemandOverTime = await httpClient
            .GetFromJsonAsync<List<ActivityPoint>>($"Insights/marketdemand/overtime?days={MarketDays}")
            ?? new List<ActivityPoint>();

        // User Activity
        UserActivities = await httpClient
            .GetFromJsonAsync<List<UserActivity>>("Insights/useractivity")
            ?? new List<UserActivity>();

        LoginActivity = await httpClient
            .GetFromJsonAsync<List<ActivityPoint>>("LoginLog/activity")
            ?? new List<ActivityPoint>();

        var dauRes = await httpClient.GetFromJsonAsync<CountResult>("LoginLog/dau");
        var wauRes = await httpClient.GetFromJsonAsync<CountResult>("LoginLog/wau");
        var mauRes = await httpClient.GetFromJsonAsync<CountResult>("LoginLog/mau");

        DAU = dauRes?.Count ?? 0;
        WAU = wauRes?.Count ?? 0;
        MAU = mauRes?.Count ?? 0;

        // Community Interest
        CommunityInterest = await httpClient
            .GetFromJsonAsync<CommunityInterestData>("Insights/communityinterest")
            ?? new CommunityInterestData();

        SearchTrends = await httpClient
            .GetFromJsonAsync<List<SearchTrend>>("SearchLog/trending")
            ?? new List<SearchTrend>();

        CommunityOverTime = await httpClient
            .GetFromJsonAsync<List<TimeSeriesPoint>>($"Insights/communityinterest/overtime?days={CommunityDays}")
            ?? new List<TimeSeriesPoint>();

        // Finance and Sales
        FinanceAndSales = await httpClient
            .GetFromJsonAsync<FinanceAndSalesDetail>("Insights/financeandsales/detail")
            ?? new FinanceAndSalesDetail();

        return Page();
    }
}