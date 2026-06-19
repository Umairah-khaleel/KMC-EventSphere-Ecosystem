using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DashboardModel : PageModel
{
    public IActionResult OnGet()
    {
        var role = HttpContext.Session.GetString("Role");

        if (role != "Organizer")
        {
            return RedirectToPage("/Login");
        }

        return Page();
    }
}