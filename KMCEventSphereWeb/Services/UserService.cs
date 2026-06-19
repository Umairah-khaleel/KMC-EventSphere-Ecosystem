using KMCEventSphereWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;

namespace KMCEventSphereWeb.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
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


        public async Task<(List<UserModel>? Users, string ErrorMessage)> GetAllUsersAsync()
        {
            AddUserHeader();

            var response = await _httpClient.GetAsync("https://localhost:7058/api/User");

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

                return (null, "Failed to load users");
            }

            var users = await response.Content.ReadFromJsonAsync<List<UserModel>>();

            return (users ?? new List<UserModel>(), "");
        }


        public async Task<UserModel> GetByIdAsync(int id)
        {
            AddUserHeader();
            return await _httpClient.GetFromJsonAsync<UserModel>($"https://localhost:7058/api/User/{id}");
        }


        public async Task AddAsync(UserModel u)
        {
            AddUserHeader();
            await _httpClient.PostAsJsonAsync("https://localhost:7058/api/User", u);
        }

        public async Task UpdateAsync(UserModel u)
        {
            await _httpClient.PutAsJsonAsync($"https://localhost:7058/api/User/{u.UserID}", u);
        }

        public async Task<(List<UserModel>? Users, string ErrorMessage)> SearchUsersAsync(string filter, string value)
        {
            AddUserHeader();

            var url = $"https://localhost:7058/api/User/search?{filter}={value}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return (null, content);
            }

            var users = await response.Content.ReadFromJsonAsync<List<UserModel>>();

            return (users ?? new List<UserModel>(), "");
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteUserAsync(int id)
        {
            AddUserHeader();
            var response = await _httpClient.DeleteAsync($"https://localhost:7058/api/User/{id}");

            var content = await response.Content.ReadAsStringAsync();

            //  Try JSON { message: "..." }
            try
            {
                var result = System.Text.Json.JsonSerializer
                    .Deserialize<Dictionary<string, string>>(content);

                if (result != null && result.ContainsKey("message"))
                {
                    if (response.IsSuccessStatusCode)
                        return (true, result["message"]);

                    return (false, result["message"]);
                }
            }
            catch { }

            // If backend returned plain text → use it
            if (!string.IsNullOrWhiteSpace(content))
            {
                if (!response.IsSuccessStatusCode)
                    return (false, content);

                return (true, content);
            }


            //  Final fallback
            if (!response.IsSuccessStatusCode)
                return (false, "Failed to delete user");

            return (true, "User deleted successfully");
        }


        public async Task<(UserModel? User, string ErrorMessage)> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7058/api/User/login",
                new { Username = username, Password = password });

            if (!response.IsSuccessStatusCode)
            {
                // Read { message: "Invalid credentials" }
                var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                if (errorObj != null && errorObj.ContainsKey("message"))
                    return (null, errorObj["message"]);

                return (null, "Login failed");
            }

            var user = await response.Content.ReadFromJsonAsync<UserModel>();
            return (user, "");
        }


        public async Task<(bool Success, string ErrorMessage)> SignupAsync(UserModel user)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7058/api/User/signup",
                user);

            if (response.IsSuccessStatusCode)
                return (true, "");

            var content = await response.Content.ReadAsStringAsync();

            // Handle { message: "Email already exists" }
            try
            {
                var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                if (errorObj != null && errorObj.ContainsKey("message"))
                    return (false, errorObj["message"]);
            }
            catch { }

            // Handle ModelState errors { errors: { field: ["msg"] } }
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

            return (false, "Signup failed");
        }

        public async Task<(bool success, string message)> UpdateUserRoleAsync(int id, string role)
        {
            var client = new HttpClient();

            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserID");

            if (userId == null)
                return (false, "Not logged in");

            client.DefaultRequestHeaders.Add("X-UserID", userId.ToString());

            var dto = new
            {
                Role = role
            };

            var response = await client.PutAsJsonAsync(
                $"https://localhost:7058/api/user/{id}/role", dto);

            if (response.IsSuccessStatusCode)
                return (true, "Role updated successfully");

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
    }
}