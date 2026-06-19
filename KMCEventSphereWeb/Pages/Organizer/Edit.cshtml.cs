using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KMCEventSphereWeb.Models;
using KMCEventSphereWeb.Services;

namespace KMCEventSphereWeb.Pages.Organizers
{
    public class EditModel : PageModel
    {
        private readonly OrganizerService _organizerService;

        public EditModel(OrganizerService organizerService)
        {
            _organizerService = organizerService;
        }

        [BindProperty]
        public OrganizerUpdateModel Organizer { get; set; } = new();

        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserID");

            OrganizerModel? data = null;

            if (role == "Admin")
            {
                if (id == null)
                    return RedirectToPage("/Admin/Organizers");

                data = await _organizerService.GetByIdAsync(id.Value);
            }
            else if (role == "Organizer")
            {
                data = await _organizerService.GetByUserIdAsync(userId.Value);
            }
            else
            {
                return RedirectToPage("/Login");
            }

            if (data == null)
            {
                ErrorMessage = "Organizer not found";
                return Page();
            }

            Organizer = new OrganizerUpdateModel
            {
                OrganizerID = data.OrganizerID,
                OrganizationName = data.OrganizationName,
                FullName = data.Name,
                Email = data.Email,
                Phone = data.Phone,
                Address = data.Address,
                Description = data.Description
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _organizerService.UpdateAsync(Organizer.OrganizerID, Organizer);

            if (result != "Organizer updated successfully")
            {
                ErrorMessage = result;
                return Page();
            }

            SuccessMessage = result;
            return Page();
        }
    }
}