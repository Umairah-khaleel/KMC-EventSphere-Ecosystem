using AutoMapper;
using KMCEventSphere.DTOs;
using KMCEventSphere.Models;
using KMCEventSphere.Data;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizerController : ControllerBase
    {
        private readonly OrganizerRepo _repo;
        private readonly UserRepo _userRepo;
        private readonly IMapper _mapper;

        public OrganizerController(OrganizerRepo repo, UserRepo userRepo, IMapper mapper)
        {
            _repo = repo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        // Get logged-in user from header (async)
        private async Task<User?> GetCurrentUserAsync()
        {
            if (Request.Headers.TryGetValue("X-UserID", out var userIdStr) &&
                int.TryParse(userIdStr, out int userId))
            {
                return await _userRepo.GetUserByIDAsync(userId);
            }
            return null;
        }

        private async Task<bool> IsAdminAsync()
        {
            var user = await GetCurrentUserAsync();
            return user != null && user.Role == "Admin";
        }

        // GET all (Admin only)
        [HttpGet]
        public async Task<ActionResult<List<OrganizerReadDTO>>> GetAll()
        {
            if (!await IsAdminAsync())
                return Unauthorized(new { message = "Admin only" });

            // Latest signup first by OrganizerID descending
            var organizers = (await _repo.GetOrganizersAsync())
                             .OrderByDescending(o => o.OrganizerID)
                             .ToList();

            return Ok(_mapper.Map<List<OrganizerReadDTO>>(organizers));
        }

        // Get by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizerReadDTO>> GetById(int id)
        {
            var org = await _repo.GetOrganizerByIDAsync(id);
            if (org == null)
                return NotFound(new { message = "Organizer not found" });

            return Ok(_mapper.Map<OrganizerReadDTO>(org));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<OrganizerReadDTO>> GetByUserId(int userId)
        {
            var org = await _repo.GetOrganizersAsync();

            var result = org.FirstOrDefault(o => o.UserID == userId);

            if (result == null)
                return NotFound(new { message = "Organizer not found" });

            return Ok(_mapper.Map<OrganizerReadDTO>(result));
        }

        // Update
        [HttpPut("{id}")]
        public async Task<ActionResult<OrganizerReadDTO>> Update(int id, OrganizerUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { message = "User not logged in" });

            var org = await _repo.GetOrganizerByIDAsync(id);
            if (org == null)
                return NotFound(new { message = "Organizer not found" });

            // ORGANIZER LOGIC
            if (user.Role == "Organizer")
            {
                if (user.Organizer == null || user.Organizer.OrganizerID != id)
                {
                    return Unauthorized(new { message = "You can only update your own profile" });
                }

                if (dto.Phone != null)
                    org.Phone = dto.Phone;

                if (dto.Address != null)
                    org.Address = dto.Address;

                if (dto.Description != null)
                    org.Description = dto.Description;
            }

            // ADMIN LOGIC
            else if (user.Role == "Admin")
            {
                // Only OrganizationName (Organizer table)
                if (dto.OrganizationName != null)
                    org.OrganizationName = dto.OrganizationName;

                // Update User table
                var userEntity = await _userRepo.GetUserByIDAsync(org.UserID);

                if (userEntity != null)
                {
                    if (dto.FullName != null)
                        userEntity.FullName = dto.FullName;

                    if (!string.IsNullOrEmpty(dto.Email))
                    {
                        var users = await _userRepo.GetUsersAsync();

                        if (users.Any(u => u.Email.ToLower().Trim() == dto.Email.ToLower().Trim()
                                           && u.UserID != userEntity.UserID))
                            return BadRequest(new { message = "Email already exists" });

                        userEntity.Email = dto.Email;
                    }

                    await _userRepo.UpdateAsync(userEntity);
                }
            }

            // OTHER ROLES
            else
            {
                return Unauthorized(new { message = "Only admin or organizer can update profile" });
            }

            await _repo.UpdateAsync(org);

            return Ok(_mapper.Map<OrganizerReadDTO>(org));
        }

        // SEARCH Method
        [HttpGet("search")]
        public async Task<ActionResult<List<OrganizerReadDTO>>> Search(
            string? key,
            string? organizationName,
            int? organizerId,
            int? userId)
        {
            if (!await IsAdminAsync())
                return Unauthorized(new { message = "Admin only" });

            var organizers = await _repo.GetOrganizersAsync();

            // KEY search (search everything)
            if (!string.IsNullOrEmpty(key))
            {
                var lowerKey = key.ToLower();

                organizers = organizers.Where(o =>
                    (o.OrganizationName != null && o.OrganizationName.ToLower().Contains(lowerKey)) ||
                    (o.Description != null && o.Description.ToLower().Contains(lowerKey)) ||
                    o.OrganizerID.ToString().Contains(key) ||
                    o.UserID.ToString().Contains(key)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(organizationName))
                organizers = organizers.Where(o =>
                    o.OrganizationName != null &&
                    o.OrganizationName.ToLower().Contains(organizationName.ToLower())
                ).ToList();

            if (organizerId.HasValue)
                organizers = organizers.Where(o => o.OrganizerID == organizerId.Value).ToList();

            if (userId.HasValue)
                organizers = organizers.Where(o => o.UserID == userId.Value).ToList();

            organizers = organizers.OrderByDescending(o => o.OrganizerID).ToList();

            return Ok(_mapper.Map<List<OrganizerReadDTO>>(organizers));
        }

        // Admin Add Organizer (creates both User + Organizer)
        [HttpPost("admin/add-organizer")]
        public async Task<ActionResult<OrganizerReadDTO>> AddOrganizer(AdminCreateOrganizerDTO dto)
        {
            if (!await IsAdminAsync())
                return Unauthorized(new { message = "Admin only" });


            // Check for existing username/email
            var users = await _userRepo.GetUsersAsync();
            if (users.Any(u => u.Username == dto.Username))
                return BadRequest(new { message = "Username already exists" });

            if (users.Any(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email already exists" });

            // Create user with role = Organizer
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Username = dto.Username,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                Role = "Organizer"
            };
            await _userRepo.CreateAsync(user);

            // Create organizer linked to user
            var organizer = new Organizer
            {
                UserID = user.UserID,
                Name = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                OrganizationName = dto.OrganizationName,
                Address = dto.Address,
                Description = dto.Description
            };
            await _repo.CreateAsync(organizer);

            return CreatedAtAction(
                nameof(GetById),
                new { id = organizer.OrganizerID },
                _mapper.Map<OrganizerReadDTO>(organizer)
             );
        }
    }

    // -----------------------------
    // Password Helper
    // -----------------------------
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}