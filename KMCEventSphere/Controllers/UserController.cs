using AutoMapper;
using KMCEventSphere.Data;
using KMCEventSphere.DTOs;
using KMCEventSphere.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace KMCEventSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepo _repo;
        private readonly IMapper _mapper;

        public UserController(UserRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // Signup
        [HttpPost("signup")]
        public async Task<ActionResult<UserReadDTO>> Signup([FromBody] UserWriteDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _repo.GetUserByUsernameAsync(dto.Username) != null)
                return BadRequest(new { message = "Username already exists" });

            var users = await _repo.GetUsersAsync();
            if (users.Any(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email already exists" });

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = HashPassword(dto.Password);
            user.Role = "Public";

            await _repo.CreateAsync(user);

            return Ok(_mapper.Map<UserReadDTO>(user));
        }

        // Login
        [HttpPost("login")]
        public async Task<ActionResult<UserReadDTO>> Login([FromBody] UserLoginDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _repo.GetUserByUsernameAsync(dto.Username);
            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(_mapper.Map<UserReadDTO>(user));
        }

        // GET all users (Admin only)
        [HttpGet]
        public async Task<ActionResult<List<UserReadDTO>>> GetAll()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await IsAdminAsync())
                return Unauthorized(new { message = "Admin only" });

            var users = await _repo.GetUsersAsync();

            // Latest signup first: order by descending UserID
            var sortedUsers = users.OrderByDescending(u => u.UserID).ToList();

            return Ok(_mapper.Map<List<UserReadDTO>>(sortedUsers));
        }

        // Update role (Admin only)
        [HttpPut("{id}/role")]
        public async Task<ActionResult<UserReadDTO>> UpdateRole(int id, [FromBody] UserRoleUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await IsAdminAsync())
                return Unauthorized(new { message = "Admin only" });

            var user = await _repo.GetUserByIDAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (user.UserID == GetCurrentUserID())
                return BadRequest(new { message = "Cannot update your own role" });

            if (dto.Role != "Public" && dto.Role != "Organizer" && dto.Role != "Admin")
                return BadRequest("Invalid role");

            user.Role = dto.Role;
            await _repo.UpdateAsync(user);

            return Ok(_mapper.Map<UserReadDTO>(user));
        }

        // Delete user (Admin only)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await IsAdminAsync()) return Unauthorized("Admin only");
            if (id == GetCurrentUserID())
                return BadRequest("Cannot delete your own account");

            var user = await _repo.GetUserByIDAsync(id);
            if (user == null) return NotFound();

            await _repo.RemoveAsync(user);
            return Ok();
        }

        [HttpGet("whoami")]
        public async Task<ActionResult<string>> WhoAmI()
        {
            int userId = GetCurrentUserID();
            var user = await _repo.GetUserByIDAsync(userId);
            if (user == null)
                return Unauthorized("No user found");

            return Ok($"UserID: {userId}, Username: {user.Username}, Role: {user.Role}");
        }

        // SEARCH USERS (Admin only)
        [HttpGet("search")]
        public async Task<ActionResult<List<UserReadDTO>>> Search(
            string? key,
            string? fullName,
            string? email,
            string? role,
            int? userId)
        {
            if (!await IsAdminAsync())
                return Unauthorized(new { message = "Admin only" });

            var users = await _repo.GetUsersAsync();

            // KEYWORD SEARCH (search EVERYTHING)
            if (!string.IsNullOrEmpty(key))
            {
                var lowerKey = key.ToLower();

                users = users.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(lowerKey)) ||
                    (u.Email != null && u.Email.ToLower().Contains(lowerKey)) ||
                    (u.Username != null && u.Username.ToLower().Contains(lowerKey)) ||
                    (u.Role != null && u.Role.ToLower().Contains(lowerKey)) ||
                    u.UserID.ToString().Contains(key)
                ).ToList();
            }

            // SPECIFIC FILTERS

            if (!string.IsNullOrEmpty(fullName))
                users = users.Where(u =>
                    u.FullName != null &&
                    u.FullName.ToLower().Contains(fullName.ToLower())
                ).ToList();

            if (!string.IsNullOrEmpty(email))
                users = users.Where(u =>
                    u.Email != null &&
                    u.Email.ToLower().Contains(email.ToLower())
                ).ToList();

            if (!string.IsNullOrEmpty(role))
                users = users.Where(u =>
                    u.Role != null &&
                    u.Role.ToLower().Contains(role.ToLower())
                ).ToList();

            if (userId.HasValue)
                users = users.Where(u => u.UserID == userId.Value).ToList();

            // Latest users first
            users = users.OrderByDescending(u => u.UserID).ToList();

            return Ok(_mapper.Map<List<UserReadDTO>>(users));
        }


        // Utils
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string password, string hash) =>
            HashPassword(password) == hash;

        private int GetCurrentUserID()
        {
            if (Request.Headers.TryGetValue("X-UserID", out var userIdStr) && int.TryParse(userIdStr, out int userId))
                return userId;
            return 0; // not logged in
        }

        private async Task<bool> IsAdminAsync()
        {
            var user = await _repo.GetUserByIDAsync(GetCurrentUserID());
            return user != null && user.Role == "Admin";
        }
    }
}