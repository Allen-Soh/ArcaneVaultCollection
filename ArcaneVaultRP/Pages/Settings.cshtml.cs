using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages;

public class SettingsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SettingsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public ArcaneVaultUser CurrentUser { get; set; } = default!;
    public List<string> AllCountries { get; set; } = CountryHelper.AllCountries;

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var sessionUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        // Always fetch fresh from API so country/avatar are current
        var freshUser = await httpClient
            .GetFromJsonAsync<ArcaneVaultUser>($"ArcaneVaultUser/{sessionUser.UserName}");

        if (freshUser == null) return RedirectToPage("/Login");

        CurrentUser = freshUser;

        // Keep session in sync
        HttpContext.Session.SetString(SessionKeys.LoggedInUser,
            JsonSerializer.Serialize(freshUser));
        HttpContext.Session.SetString(SessionKeys.UserRole,
            freshUser.RoleId.ToString());

        return Page();
    }

    public async Task<IActionResult> OnPostRefreshSessionAsync([FromQuery] string newUsername)
    {
        if (string.IsNullOrEmpty(newUsername))
            return BadRequest();

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);


        ArcaneVaultUser? freshUser;
        try
        {
            freshUser = await httpClient
                .GetFromJsonAsync<ArcaneVaultUser>($"ArcaneVaultUser/{newUsername}");
        }
        catch (HttpRequestException)
        {
            return BadRequest();
        }

        if (freshUser == null) return BadRequest();

        // Update session with new username
        HttpContext.Session.SetString(SessionKeys.LoggedInUser,
            JsonSerializer.Serialize(freshUser));
        HttpContext.Session.SetString(SessionKeys.UserRole,
            freshUser.RoleId.ToString());

        return new OkResult();
    }
}