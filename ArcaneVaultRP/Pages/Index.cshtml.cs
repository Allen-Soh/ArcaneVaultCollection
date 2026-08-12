//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // Collector (non-staff) stats
    public int TotalShowoffs { get; set; }
    public int TotalLikes { get; set; }
    public int TotalComments { get; set; }
    public ShowoffSummary? TopShowoff { get; set; }

    // Staff stats
    public int PendingReportsCount { get; set; }
    public int PendingTicketsCount { get; set; }

    public async Task OnGetAsync()
    {
        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        if (userJson == null) return;

        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);
        if (loggedInUser == null) return;

        var isStaff = HttpContext.Session.GetString(SessionKeys.UserRole) == "1";

        var httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        if (isStaff)
            await LoadStaffStatsAsync(httpClient, loggedInUser.UserName);
        else
            await LoadCollectorStatsAsync(httpClient, loggedInUser.UserName);
    }

    private async Task LoadCollectorStatsAsync(HttpClient httpClient, string userName)
    {
        try
        {
            var showoffs = await httpClient.GetFromJsonAsync<List<ShowoffSummary>>(
                $"Showoff/user/{userName}?requester={userName}");

            if (showoffs == null) return;

            TotalShowoffs = showoffs.Count;
            TotalLikes = showoffs.Sum(s => s.LikeCount);
            TotalComments = showoffs.Sum(s => s.CommentCount);

            TopShowoff = showoffs
                .OrderByDescending(s => s.LikeCount)
                .ThenByDescending(s => s.CommentCount)
                .FirstOrDefault(s => s.LikeCount > 0 || s.CommentCount > 0);
        }
        catch (HttpRequestException)
        {
            // API unreachable — leave stats at zero rather than failing the whole home page
        }
    }

    private async Task LoadStaffStatsAsync(HttpClient httpClient, string userName)
    {
        try
        {
            var reports = await httpClient.GetFromJsonAsync<List<ResolvableSummary>>(
                $"ShowoffReport/staff/{userName}");
            PendingReportsCount = reports?.Count(r => !r.IsResolved) ?? 0;
        }
        catch (HttpRequestException) { }

        try
        {
            var tickets = await httpClient.GetFromJsonAsync<List<ResolvableSummary>>(
                $"SupportTicket/staff/{userName}");
            PendingTicketsCount = tickets?.Count(t => !t.IsResolved) ?? 0;
        }
        catch (HttpRequestException) { }
    }

    // Minimal shapes — System.Text.Json ignores extra fields in the response, so these
    // deliberately only declare what the home page cards actually need.
    public class ShowoffSummary
    {
        public int ShowoffId { get; set; }
        public string ShowoffName { get; set; } = "";
        public string? BackgroundColor { get; set; }
        public string? BackgroundImagePath { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
    }

    public class ResolvableSummary
    {
        public bool IsResolved { get; set; }
    }
}