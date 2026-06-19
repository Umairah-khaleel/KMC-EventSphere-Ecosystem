using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMCEventSphereWeb.Pages
{
    public class IndexModel : PageModel
    {
        public bool IsLoggedIn { get; set; }
        public string Role { get; set; } = "";

        public void OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            IsLoggedIn = userId != null;
            Role = role ?? "";
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        public IActionResult OnPostGoDashboard()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role == "Admin")
                return RedirectToPage("/Admin/Dashboard");

            if (role == "Organizer")
                return RedirectToPage("/Organizer/Dashboard");

            return RedirectToPage("/Public/Dashboard");
        }
    }
}