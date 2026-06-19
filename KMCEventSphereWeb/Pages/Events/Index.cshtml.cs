using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KMCEventSphereWeb.Models;
using KMCEventSphereWeb.Services;

namespace KMCEventSphereWeb.Pages.Events
{
    public class IndexModel : PageModel
    {
        private readonly EventService _eventService;

        public IndexModel(EventService eventService)
        {
            _eventService = eventService;
        }

        public List<EventModel> Events { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "key";

        [BindProperty(SupportsGet = true)]
        public string Value { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            // LOGIN CHECK
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                ErrorMessage = "User not logged in";
                return RedirectToPage("/Login");
            }

            // IF SEARCH USED
            if (!string.IsNullOrEmpty(Value))
            {
                var result = await _eventService.SearchAsync(Filter, Value);

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    ErrorMessage = result.ErrorMessage;

                    // If API says not logged in → redirect
                    if (ErrorMessage.Contains("not logged in", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToPage("/Login");
                    }

                    return Page();
                }

                Events = result.Events ?? new List<EventModel>();
                return Page();
            }

            // NORMAL LOAD
            var normal = await _eventService.GetAllAsync();

            if (!string.IsNullOrEmpty(normal.ErrorMessage))
            {
                ErrorMessage = normal.ErrorMessage;

                // If API says not logged in → redirect
                if (ErrorMessage.Contains("not logged in", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToPage("/Login");
                }

                return Page();
            }

            Events = normal.Events ?? new List<EventModel>();

            return Page();
        }
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToPage("/Login");
            }

            var result = await _eventService.DeleteAsync(id);

            if (result != "Event deleted successfully")
            {
                ErrorMessage = result;
            }
            else
            {
                ErrorMessage = null;
                TempData["Message"] = result; // success message
            }

            return RedirectToPage(); // refresh page
        }

    }

}