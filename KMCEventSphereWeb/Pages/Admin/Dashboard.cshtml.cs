using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMCEventSphereWeb.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Check login
            if (HttpContext.Session.GetInt32("UserID") == null)
                return RedirectToPage("/Login");

            // Check role
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
                return RedirectToPage("/Events/Index");

            return Page();
        }
    }
}