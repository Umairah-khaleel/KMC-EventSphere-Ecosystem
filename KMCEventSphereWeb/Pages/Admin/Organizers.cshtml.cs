using KMCEventSphereWeb.Models;
using KMCEventSphereWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMCEventSphereWeb.Pages.Admin
{
    public class OrganizersModel : PageModel
    {
        private readonly OrganizerService _organizerService;

        public OrganizersModel(OrganizerService organizerService)
        {
            _organizerService = organizerService;
        }

        public List<OrganizerModel> Organizers { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

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
                var result = await _organizerService.SearchOrganizersAsync(Filter, Value);

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    ErrorMessage = result.ErrorMessage;
                    return Page();
                }

                Organizers = result.Organizers ?? new List<OrganizerModel>();
                return Page();
            }

            var normal = await _organizerService.GetAllOrganizersAsync();

            if (!string.IsNullOrEmpty(normal.ErrorMessage))
            {
                ErrorMessage = normal.ErrorMessage;
                return Page();
            }

            Organizers = normal.Organizers ?? new List<OrganizerModel>();

            return Page();
        }
    }
}