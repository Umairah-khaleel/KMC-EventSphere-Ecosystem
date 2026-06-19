using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

public class EventsModel : PageModel
{
    public List<EventDTO> Events { get; set; } = new();

    // LOAD EVENTS 
    public async Task OnGetAsync()
    {
        var client = new HttpClient();

        var userId = HttpContext.Session.GetInt32("UserID");

        if (userId != null)
        {
            client.DefaultRequestHeaders.Add("X-UserID", userId.ToString());
        }

        var response = await client.GetAsync("https://localhost:7058/api/event");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Events = JsonSerializer.Deserialize<List<EventDTO>>(json, options);

            var role = HttpContext.Session.GetString("Role");

            // If NOT logged in → show only Tech Events
            if (string.IsNullOrEmpty(role))
            {
                const int TECHEVENTS_ID = 1;

                Events = Events.Where(e => e.OrganizerID == TECHEVENTS_ID).ToList();
            }
        }
    }

    //DELETE EVENT 
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserID");

        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-UserID", userId.ToString());

        var response = await client.DeleteAsync($"https://localhost:7058/api/event/{id}");

        var json = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<ApiResponse>(json, options);

        if (result == null || result.Message != "Event deleted successfully")
        {
            TempData["Error"] = result?.Message ?? "Delete failed";
        }
        else
        {
            TempData["Message"] = result.Message;
        }

        return RedirectToPage();
    }
}


// DTO
public class EventDTO
{
    public int EventID { get; set; }
    public int OrganizerID { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? Date { get; set; }
    public TimeSpan? Time { get; set; }
    public string Location { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int SeatsAvailable { get; set; }
    public int RegistrationCount { get; set; }
}

// API RESPONSE MODEL 
public class ApiResponse
{
    public string Message { get; set; }
}