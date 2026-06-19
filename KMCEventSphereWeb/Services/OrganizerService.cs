using KMCEventSphereWeb.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace KMCEventSphereWeb.Services
{
    public class OrganizerService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrganizerService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
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

        public async Task<(List<OrganizerModel>? Organizers, string ErrorMessage)> GetAllOrganizersAsync()
        {
            AddUserHeader();

            var response = await _httpClient.GetAsync("https://localhost:7058/api/Organizer");

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var errorObj = System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, string>>(content);

                    if (errorObj != null && errorObj.ContainsKey("message"))
                        return (null, errorObj["message"]);
                }
                catch { }

                return (null, "Failed to load organizers");
            }

            var organizers = await response.Content.ReadFromJsonAsync<List<OrganizerModel>>();

            return (organizers ?? new List<OrganizerModel>(), "");
        }

        public async Task<OrganizerModel> GetByIdAsync(int id)
        {
            AddUserHeader();
            return await _httpClient.GetFromJsonAsync<OrganizerModel>($"https://localhost:7058/api/Organizer/{id}");
        }

        public async Task<OrganizerModel?> GetByUserIdAsync(int userId)
        {
            AddUserHeader();

            var response = await _httpClient.GetAsync(
                $"https://localhost:7058/api/Organizer/user/{userId}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<OrganizerModel>();
        }

        public async Task<(bool Success, string ErrorMessage)> AddOrganizerAsync(AdminCreateOrganizerModel model)
        {
            AddUserHeader();

            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7058/api/Organizer/admin/add-organizer",
                model
            );

            // SUCCESS
            if (response.IsSuccessStatusCode)
                return (true, "Organizer added successfully!");

            var content = await response.Content.ReadAsStringAsync();

            // Handle { message: "Username already exists" }
            try
            {
                var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                if (errorObj != null && errorObj.ContainsKey("message"))
                    return (false, errorObj["message"]);
            }
            catch { }

            // Handle ModelState errors
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("errors", out var errors))
                {
                    foreach (var field in errors.EnumerateObject())
                    {
                        var firstError = field.Value[0].GetString();
                        if (!string.IsNullOrEmpty(firstError))
                            return (false, firstError);
                    }
                }
            }
            catch { }

            // Fallback
            return (false, "Failed to add organizer");
        }

        public async Task<string> UpdateAsync(int id, OrganizerUpdateModel model)
        {
            AddUserHeader();

            var response = await _httpClient.PutAsJsonAsync(
                $"https://localhost:7058/api/Organizer/{id}", model);

            if (response.IsSuccessStatusCode)
                return "Organizer updated successfully";

            var content = await response.Content.ReadAsStringAsync();

            // { message: "..." }
            try
            {
                var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                if (errorObj != null && errorObj.ContainsKey("message"))
                    return errorObj["message"];
            }
            catch { }

            // ModelState errors
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

            return "Failed to update organizer details";
        }


        public async Task<(List<OrganizerModel>? Organizers, string ErrorMessage)> SearchOrganizersAsync(string filter, string value)
        {
            AddUserHeader();

            var cleanFilter = filter?.Trim().ToLower();

            if (string.IsNullOrEmpty(cleanFilter))
                cleanFilter = "key"; // fallback

            var url = $"https://localhost:7058/api/Organizer/search?{cleanFilter}={value}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return (null, content);
            }

            var organizers = await response.Content.ReadFromJsonAsync<List<OrganizerModel>>();

            return (organizers ?? new List<OrganizerModel>(), "");
        }

        public async Task DeleteAsync(int id)
        {
            AddUserHeader();
            await _httpClient.DeleteAsync($"https://localhost:7058/api/Organizer/{id}");
        }
    }
}