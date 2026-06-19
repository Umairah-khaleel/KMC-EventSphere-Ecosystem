using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Username { get; set; }

    [BindProperty]
    public string Password { get; set; }

    public string ErrorMessage { get; set; }

    public string SuccessMessage { get; set; }

    public IActionResult OnPost()
    {
        if (Username == "tech1" && Password == "tech123")
        {
            HttpContext.Session.SetInt32("UserID", 5);
            HttpContext.Session.SetString("Role", "Organizer");

            SuccessMessage = "Login Successful! Redirecting to dashboard...";
            return Page();
        }

        ErrorMessage = "Invalid username or password";
        return Page();
    }
}