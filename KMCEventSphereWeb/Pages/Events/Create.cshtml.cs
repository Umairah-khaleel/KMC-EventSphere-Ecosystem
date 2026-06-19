using KMCEventSphereWeb.Models;
using KMCEventSphereWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMCEventSphereWeb.Pages.Events
{
    public class CreateModel : PageModel
    {
        private readonly EventService _service;

        public CreateModel(EventService service)
        {
            _service = service;
        }

        [BindProperty]
        public EventModel Event { get; set; } = new();

        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Role") != "Organizer")
                return RedirectToPage("/Login");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Event.Date.HasValue && Event.Date.Value.Date <= DateTime.Today)
            {
                ModelState.AddModelError("Event.Date", "Date must be in the future");
                return Page();
            }

            //  COMBINE DATE + TIME
            var combinedDateTime = Event.Date!.Value.Date + Event.Time!.Value;
            Event.Date = combinedDateTime;

            var result = await _service.AddEventAsync(Event);

            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return Page();
            }

            SuccessMessage = "Event created successfully!";
            return Page();

        }


    }
}

