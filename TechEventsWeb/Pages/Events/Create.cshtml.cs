using KMCEventSphereWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

public class CreateModel : PageModel
{
    [BindProperty]
    public EventModel Event { get; set; } = new();


    public async Task<IActionResult> OnPostAsync()
    {
        //  VALIDATION FIRST
        if (!ModelState.IsValid)
            return Page();


        if (Event.Date!.Value.Date <= DateTime.Today)
        {
            ModelState.AddModelError("Event.Date", "Date must be in the future");
            return Page();
        }

        // COMBINE DATE + TIME
        Event.Date = Event.Date!.Value.Date + Event.Time!.Value;

        // MAP Model ➜ DTO
        var dto = new EventDTO
        {
            Title = Event.Title,
            Description = Event.Description,
            Date = Event.Date,
            Location = Event.Location,
            Category = Event.Category,
            Price = Event.Price,
            SeatsAvailable = Event.SeatsAvailable
        };

        // CHECK LOGIN
        var userId = HttpContext.Session.GetInt32("UserID");

        if (userId == null)
        {
                TempData["Error"] = "User not logged in";
                return Page();
        }

        // SEND TO API
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-UserID", userId.ToString());

        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://localhost:7058/api/event", content);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] =  "Failed to create event";
            return Page();
        }

        TempData["Message"] = "Event created successfully!";
        Event = new EventModel();
        return Page();
    }
}