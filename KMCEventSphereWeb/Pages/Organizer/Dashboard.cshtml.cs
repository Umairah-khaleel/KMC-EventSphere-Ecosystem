using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMCEventSphereWeb.Pages.Organizer
{
    public class DashboardModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
                return RedirectToPage("/Login");

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organizer")
                return RedirectToPage("/Events/Index");

            return Page();
        }
    }
}