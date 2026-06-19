using KMCEventSphereWeb.Models;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace KMCEventSphereWeb.Services
{
    public class EventService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EventService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper method to add X-UserID header from session
        private void AddUserHeader()
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserID");

            if (userId != null)
            {
                // Remove existing header to avoid duplicates
                _httpClient.DefaultRequestHeaders.Remove("X-UserID");
                _httpClient.DefaultRequestHeaders.Add("X-UserID", userId.ToString());
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddEventAsync(EventModel model)
        {
            AddUserHeader();

            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7058/api/Event",
                model
            );

            if (response.IsSuccessStatusCode)
                return (true, "Event added successfully!");

            var content = await response.Content.ReadAsStringAsync();

            //  { message: "..." }
            try
            {
                var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                if (errorObj != null && errorObj.ContainsKey("message"))
                    return (false, errorObj["message"]);
            }
            catch { }

            //  ModelState errors { errors: { field: ["msg"] } }
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("errors", out var errors))
                {
                    foreach (var field in errors.EnumerateObject())
                    {
                        foreach (var err in field.Value.EnumerateArray())
                        {
                            var msg = err.GetString();
                            if (!string.IsNullOrEmpty(msg))
                                return (false, msg);
                        }
                    }
                }
            }
            catch { }

            // Fallback
            if (!string.IsNullOrWhiteSpace(content))
                return (false, content);

            return (false, "Failed to add event");
        }

        public async Task<(List<EventModel>? Events, string ErrorMessage)> GetAllAsync()
        {
            AddUserHeader();

            var response = await _httpClient.GetAsync("https://localhost:7058/api/Event");

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Handle { message: "something" }
                    var errorObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(content);

                    if (errorObj != null && errorObj.ContainsKey("message"))
                        return (null, errorObj["message"]);
                }
                catch { }

                return (null, "Failed to load events");
            }

            var events = await response.Content.ReadFromJsonAsync<List<EventModel>>();

            return (events ?? new List<EventModel>(), "");
        }

        public async Task<EventModel?> GetByIdAsync(int id)
        {
            AddUserHeader();

            var response = await _httpClient.GetAsync(
                $"https://localhost:7058/api/Event/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<EventModel>();
        }

        public async Task AddAsync(EventModel e)
        {
            AddUserHeader();
            await _httpClient.PostAsJsonAsync("https://localhost:7058/api/Event", e);
        }



        // UPDATE
        public async Task<string> UpdateAsync(EventModel ev)
        {
            AddUserHeader();

            var response = await _httpClient.PutAsJsonAsync(
                $"https://localhost:7058/api/Event/{ev.EventID}", ev);

            if (response.IsSuccessStatusCode)
                return "Event updated successfully"; // success

            var content = await response.Content.ReadAsStringAsync();

            // Handle { message: "error" }
            try
            {
                var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                if (errorObj != null && errorObj.ContainsKey("message"))
                    return errorObj["message"];
            }
            catch { }

            // Handle ModelState { errors: { field: ["msg"] } }
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("errors", out var errors))
                {
                    foreach (var field in errors.EnumerateObject())
                    {
                        var firstError = field.Value[0].GetString();
                        if (!string.IsNullOrEmpty(firstError))
                            return firstError;
                    }
                }
            }
            catch { }

            return "Event update failed";
        }

        public async Task<string> DeleteAsync(int id)
        {
            AddUserHeader();

            var response = await _httpClient.DeleteAsync(
                $"https://localhost:7058/api/Event/{id}");

            if (response.IsSuccessStatusCode)
                return "Event deleted successfully";

            var content = await response.Content.ReadAsStringAsync();

            // { message: "error" }
            try
            {
                var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                if (errorObj != null && errorObj.ContainsKey("message"))
                    return errorObj["message"];
            }
            catch { }

            return "Delete failed";
        }

        public async Task<(List<EventModel>? Events, string ErrorMessage)> SearchAsync(string filter, string value)
        {
            AddUserHeader();

            var url = $"https://localhost:7058/api/Event/search?{filter}={value}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                    if (errorObj != null && errorObj.ContainsKey("message"))
                        return (null, errorObj["message"]);
                }
                catch { }

                return (null, "Search failed");
            }

            var events = await response.Content.ReadFromJsonAsync<List<EventModel>>();
            return (events, "");
        }
    }
}