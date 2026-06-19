using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KMCEventSphereWeb.Models;
using KMCEventSphereWeb.Services;

namespace KMCEventSphereWeb.Pages.Events
{
    public class EditModel : PageModel
    {
        private readonly EventService _eventService;

        public EditModel(EventService eventService)
        {
            _eventService = eventService;
        }

        [BindProperty]
        public EventModel Event { get; set; } = new();

        [BindProperty]
        public TimeSpan Time { get; set; }

        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";

        // 🔹 LOAD EVENT
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var ev = await _eventService.GetByIdAsync(id);

            if (ev == null)
            {
                ErrorMessage = "Event not found";
                return Page();
            }

            Event = ev;

            // Extract time safely
            Time = Event.Date?.TimeOfDay ?? TimeSpan.Zero;

            return Page();
        }

        // 🔹 UPDATE EVENT
        public async Task<IActionResult> OnPostAsync()
        {
            Event.Date = (Event.Date ?? DateTime.Now).Date + Time;

            var result = await _eventService.UpdateAsync(Event);

            if (result != "Event updated successfully")
            {
                ErrorMessage = result;
                return Page();
            }

            SuccessMessage = result;
            return Page();
        }
    }
}