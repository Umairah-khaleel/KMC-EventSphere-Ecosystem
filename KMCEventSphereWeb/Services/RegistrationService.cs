using KMCEventSphereWeb.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace KMCEventSphereWeb.Services
{
    public class RegistrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RegistrationService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
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

        public async Task<(List<RegistrationModel>? Registrations, string ErrorMessage)> GetAllAsync()
        {
            AddUserHeader();

            var response = await _httpClient.GetAsync("https://localhost:7058/api/Registration");

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

                return (null, "Failed to load registrations");
            }

            var data = await response.Content.ReadFromJsonAsync<List<RegistrationModel>>();

            return (data ?? new List<RegistrationModel>(), "");
        }

        public async Task<RegistrationModel> GetByIdAsync(int id)
        {
            AddUserHeader();
            return await _httpClient.GetFromJsonAsync<RegistrationModel>($"https://localhost:7058/api/Registration/{id}");
        }

        public async Task<(bool Success, string Message)> RegisterAsync(int eventId, RegistrationModel registration)
        {
            AddUserHeader();

            var response = await _httpClient.PostAsJsonAsync(
               $"https://localhost:7058/api/Registration/{eventId}", //  eventId moved to URL
               new
               {
                   participantName = registration.ParticipantName,   // send name from form
                   participantEmail = registration.ParticipantEmail  // send email form form
               });

            //  ERROR
            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                    if (errorObj != null && errorObj.ContainsKey("message"))
                        return (false, errorObj["message"]);
                }
                catch { }

                return (false, "Registration failed");
            }

            // SUCCESS
            return (true, "Registration successful");
        }

        public async Task AddAsync(RegistrationModel r)
        {
            AddUserHeader();
            await _httpClient.PostAsJsonAsync("https://localhost:7058/api/Registration", r);
        }

        //Search
        public async Task<(List<RegistrationModel>?, string)> SearchAsync(string filter, string value)
        {
            AddUserHeader();

            var url = $"https://localhost:7058/api/Registration/search?{filter}={value}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return (null, content);
            }

            var data = await response.Content.ReadFromJsonAsync<List<RegistrationModel>>();
            return (data ?? new List<RegistrationModel>(), "");
        }

    }
}