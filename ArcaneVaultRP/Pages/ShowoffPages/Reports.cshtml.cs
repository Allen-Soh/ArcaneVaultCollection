using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.ShowoffPages;

public class ReportsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ReportsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string ActiveTab { get; set; } = "all";
    public List<ShowoffReport> AllReports { get; set; } = new();
    public List<ShowoffReport> MyReports { get; set; } = new();
    public string? CurrentStaffUserName { get; set; }

    public async Task<IActionResult> OnGetAsync(string tab = "all")
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        // Staff only (RoleId == 1)
        var userRole = HttpContext.Session.GetString(SessionKeys.UserRole);
        if (userRole != "1")
            return RedirectToPage("/Index");

        CurrentStaffUserName = loggedInUser?.UserName;
        ActiveTab = tab;

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        AllReports = await httpClient.GetFromJsonAsync<List<ShowoffReport>>("ShowoffReport")
                     ?? new List<ShowoffReport>();

        MyReports = await httpClient.GetFromJsonAsync<List<ShowoffReport>>($"ShowoffReport/staff/{CurrentStaffUserName}")
                    ?? new List<ShowoffReport>();

        return Page();
    }
}
