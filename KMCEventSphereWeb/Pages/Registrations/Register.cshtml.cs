using KMCEventSphereWeb.Models;
using KMCEventSphereWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMCEventSphereWeb.Pages.Registrations
{
    public class RegisterModel : PageModel
    {
        private readonly RegistrationService _registrationService;

        public RegisterModel(RegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        // Get Event ID from URL
        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty]
        public RegistrationModel Registration { get; set; } = new();

        public string Message { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        // Page load
        public IActionResult OnGet()
        {
            // 🔐 Check login
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }

        // Handle registration
        public async Task<IActionResult> OnPostAsync()
        {
            // Check login again
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToPage("/Login");
            }

            // Call API via service
            var result = await _registrationService.RegisterAsync(EventId, Registration);

            if (result.Success)
            {
                Message = result.Message;       // "Registration successful"
                ErrorMessage = "";
                Registration = new RegistrationModel();
            }
            else
            {
                ErrorMessage = result.Message; // backend error
                Message = "";
            }

            return Page();
        }
    }
}