using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KMCEventSphereWeb.Models;
using KMCEventSphereWeb.Services;

public class SignupModel : PageModel
{
    private readonly UserService _userService;

    public SignupModel(UserService userService)
    {
        _userService = userService;
    }

    [BindProperty]
    public UserModel User { get; set; } = new UserModel();

    public string ErrorMessage { get; set; } = "";
    public string SuccessMessage { get; set; } = "";

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // FRONTEND VALIDATION
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _userService.SignupAsync(User);

        if (!result.Success)
        {
            ErrorMessage = result.ErrorMessage; // show API error
            return Page();
        }

        SuccessMessage = "Signup Successful! Redirecting to login...";
        return Page();
    }
}