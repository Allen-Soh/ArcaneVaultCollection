//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.InsightPages;

public class CommunityInterestModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CommunityInterestModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public CommunityInterestDetail Detail { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int Days { get; set; } = 7;

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

        Detail = await httpClient
            .GetFromJsonAsync<CommunityInterestDetail>(
                $"Insights/communityinterest/detail?days={Days}")
            ?? new CommunityInterestDetail();

        return Page();
    }
}