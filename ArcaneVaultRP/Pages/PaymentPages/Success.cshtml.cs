//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.PaymentPages;

public class SuccessModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SuccessModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public bool Found { get; set; }
    public string? CosmeticName { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Status { get; set; }

    public async Task<IActionResult> OnGetAsync([FromQuery(Name = "session_id")] string? sessionId)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        if (string.IsNullOrEmpty(sessionId))
            return Page();

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        var response = await httpClient.GetAsync($"Payment/by-session/{sessionId}");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<PaymentLookupResult>();
            if (data != null)
            {
                Found = true;
                CosmeticName = data.CosmeticName;
                Amount = data.Amount;
                Currency = data.Currency;
                Status = data.Status;
            }
        }

        return Page();
    }

    public class PaymentLookupResult
    {
        public string CosmeticName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
    }
}
