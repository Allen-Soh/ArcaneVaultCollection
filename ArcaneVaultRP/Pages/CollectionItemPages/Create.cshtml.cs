using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.CollectionItemPages;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult OnGet()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
        {
            return RedirectToPage("/Login");
        }
        return Page();
    }

    [BindProperty]
    public CollectionItem CollectionItem { get; set; } = default!;

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("CollectionItem.UserName");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        var dto = new CollectionItem
        {
            ItemName = CollectionItem.ItemName,
            IsDeleted = false,
            StartingQuantity = CollectionItem.StartingQuantity,
            CurrentQuantity = CollectionItem.StartingQuantity,
            UserName = loggedInUser.UserName
        };

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var response = await httpClient.PostAsJsonAsync("CollectionItem", dto);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "An error occurred while creating the collection item.");
            return Page();
        }
    }
}