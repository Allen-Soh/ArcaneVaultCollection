//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.SupportPages;

public class StaffModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public StaffModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string ActiveTab { get; set; } = "all";
    public string CurrentStaffUserName { get; set; } = "";
    public List<SupportTicket> AllTickets { get; set; } = new();
    public List<SupportTicket> MyTickets { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string tab = "all")
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        if (HttpContext.Session.GetString(SessionKeys.UserRole) != "1")
            return RedirectToPage("/ShowoffPages/Index");

        CurrentStaffUserName = loggedInUser.UserName;
        ActiveTab = tab;

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        AllTickets = await httpClient.GetFromJsonAsync<List<SupportTicket>>("SupportTicket")
                     ?? new List<SupportTicket>();

        MyTickets = await httpClient.GetFromJsonAsync<List<SupportTicket>>($"SupportTicket/staff/{loggedInUser.UserName}")
                    ?? new List<SupportTicket>();

        return Page();
    }
}