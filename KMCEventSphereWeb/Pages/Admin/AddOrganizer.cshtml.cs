using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KMCEventSphereWeb.Models;
using KMCEventSphereWeb.Services;

namespace KMCEventSphereWeb.Pages.Admin
{
    public class AddOrganizerModel : PageModel
    {
        private readonly OrganizerService _service;

        public AddOrganizerModel(OrganizerService service)
        {
            _service = service;
        }

        [BindProperty]
        public AdminCreateOrganizerModel Organizer { get; set; } = new();

        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _service.AddOrganizerAsync(Organizer);

            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return Page();
            }

            SuccessMessage = "Organizer created successfully!";
            return RedirectToPage("/Admin/Organizers");
        }
    }
}