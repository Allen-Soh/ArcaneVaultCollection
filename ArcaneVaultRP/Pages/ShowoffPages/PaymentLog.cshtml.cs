//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.ShowoffPages;

public class PaymentLogModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentLogModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<Payment> Payments { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        // Staff only (RoleId == 1)
        var userRole = HttpContext.Session.GetString(SessionKeys.UserRole);
        if (userRole != "1")
            return RedirectToPage("/Index");

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        Payments = await httpClient.GetFromJsonAsync<List<Payment>>("Payment")
                   ?? new List<Payment>();

        return Page();
    }
}
