using KMCEventSphereWeb.Models;
using KMCEventSphereWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMCEventSphereWeb.Pages.Registrations
{
    public class IndexModel : PageModel
    {
        private readonly RegistrationService _service;

        public IndexModel(RegistrationService service)
        {
            _service = service;
        }

        public List<RegistrationModel> Registrations { get; set; } = new();
        public string ErrorMessage { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "key";

        [BindProperty(SupportsGet = true)]
        public string Value { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
                return RedirectToPage("/Login");

            if (!string.IsNullOrEmpty(Value))
            {
                var result = await _service.SearchAsync(Filter, Value);
                if (!string.IsNullOrEmpty(result.Item2))
                {
                    ErrorMessage = result.Item2;
                    return Page();
                }

                Registrations = result.Item1 ?? new();
                return Page();
            }

            var normal = await _service.GetAllAsync();

            if (!string.IsNullOrEmpty(normal.Item2))
            {
                ErrorMessage = normal.Item2;
                return Page();
            }

            Registrations = normal.Item1 ?? new();

            return Page();
        }
    }
}