//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.CategoryPages;

public class DeleteModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DeleteModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public Category Category { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string? categorycode)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
        {
            return RedirectToPage("/Login");
        }
        if (HttpContext.Session.GetString(SessionKeys.UserRole) != "1")
        {
            return RedirectToPage("/Index");
        }

        if (categorycode is null)
        {
            return NotFound();
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        // LINQ — find category by code
        var allCategories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                            ?? new List<Category>();

        var category = allCategories
            .FirstOrDefault(c => c.CategoryCode == categorycode);

        if (category is null)
        {
            return NotFound();
        }

        Category = category;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");

        // Attach JWT token from session
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        var response = await httpClient.DeleteAsync($"Category/{Category.CategoryCode}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "An error occurred while deleting the category.");
            return Page();
        }
    }
}