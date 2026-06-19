using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KMCEventSphereWeb.Services;

public class LoginModel : PageModel
{
    private readonly UserService _userService;

    public LoginModel(UserService userService)
    {
        _userService = userService;
    }

    [BindProperty]
    public string Username { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    public string ErrorMessage { get; set; } = "";
    public string SuccessMessage { get; set; } = "";
    public string RedirectUrl { get; set; } = "";

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Username and Password are required";
            return Page();
        }

        var result = await _userService.LoginAsync(Username, Password);

        if (result.User == null)
        {
            ErrorMessage = result.ErrorMessage;
            return Page();
        }

        SuccessMessage = "Login Successful! Redirecting to dashboard...";

        var user = result.User;

        // STORE SESSION
        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("Role", user.Role);

        // ROLE-BASED REDIRECT
        if (user.Role == "Admin")
            RedirectUrl = "/Admin/Dashboard";
        else if (user.Role == "Organizer")
            RedirectUrl = "/Organizer/Dashboard";
        else
            RedirectUrl = "/Public/Dashboard";

        return Page();
    }
}