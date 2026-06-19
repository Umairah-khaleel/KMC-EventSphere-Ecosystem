using KMCEventSphereWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

public class EditModel : PageModel
{
    [BindProperty]
    public EventModel Event { get; set; } = new();

    [BindProperty]
    public TimeSpan Time { get; set; }

    // ================= LOAD EVENT =================
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = new HttpClient();

        var userId = HttpContext.Session.GetInt32("UserID");

        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        client.DefaultRequestHeaders.Add("X-UserID", userId.ToString());

        var response = await client.GetAsync($"https://localhost:7058/api/event/{id}");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Failed to load event";
            return RedirectToPage("/Events/Index");
        }

        var json = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var dto = JsonSerializer.Deserialize<EventDTO>(json, options);

        // MAP DTO ➜ MODEL 
        Event = new EventModel
        {
            EventID = dto.EventID,
            Title = dto.Title,
            Description = dto.Description,
            Date = dto.Date,
            Time = dto.Date?.TimeOfDay,
            Location = dto.Location,
            Category = dto.Category,
            Price = dto.Price,
            SeatsAvailable = dto.SeatsAvailable
        };

        // extract time
        Time = Event.Date?.TimeOfDay ?? TimeSpan.Zero;

        return Page();
    }

    // ================= UPDATE EVENT =================
    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserID");

        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        // combine date + time
        Event.Date = Event.Date!.Value.Date + Time;

        // MAP MODEL ➜ DTO
        var dto = new EventDTO
        {
            EventID = Event.EventID,
            Title = Event.Title,
            Description = Event.Description,
            Date = Event.Date,
            Location = Event.Location,
            Category = Event.Category,
            Price = Event.Price,
            SeatsAvailable = Event.SeatsAvailable
        };

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-UserID", userId.ToString());

        var response = await client.PutAsJsonAsync(
            $"https://localhost:7058/api/event/{dto.EventID}", dto);

        // Success
        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Event updated successfully";
            return Page(); 
        }

        var content = await response.Content.ReadAsStringAsync();

        // Handle { message: "..." }
        try
        {
            var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

            if (errorObj != null && errorObj.ContainsKey("message"))
            {
                TempData["Error"] = errorObj["message"];
                return Page(); 
            }
        }
        catch { }

        // Handle ModelState { errors: { field: ["msg"] } }
        try
        {
            using var doc = JsonDocument.Parse(content);

            if (doc.RootElement.TryGetProperty("errors", out var errors))
            {
                foreach (var field in errors.EnumerateObject())
                {
                    var firstError = field.Value[0].GetString();
                    if (!string.IsNullOrEmpty(firstError))
                    {
                        TempData["Error"] = firstError;
                        return Page();
                    }
                }
            }
        }
        catch { }

        // Final fallback
        TempData["Error"] = "Event update failed";

        return Page();
    }
}
