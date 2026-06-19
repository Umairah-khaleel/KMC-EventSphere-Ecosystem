using Microsoft.AspNetCore.Mvc;
using KMCEventSphere.Data;
using KMCEventSphere.Models;
using KMCEventSphere.DTOs;
using AutoMapper;

namespace KMCEventSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly RegistrationRepo _regRepo;
        private readonly EventRepo _eventRepo;
        private readonly UserRepo _userRepo;
        private readonly IMapper _mapper;

        public RegistrationController(RegistrationRepo regRepo, EventRepo eventRepo, UserRepo userRepo, IMapper mapper)
        {
            _regRepo = regRepo;
            _eventRepo = eventRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        // Get current logged-in user (async)
        private async Task<User?> GetCurrentUserAsync()
        {
            if (Request.Headers.TryGetValue("X-UserID", out var userIdStr) &&
                int.TryParse(userIdStr, out int userId))
            {
                return await _userRepo.GetUserByIDAsync(userId);
            }
            return null;
        }

        // CREATE registration
        [HttpPost("{eventId}")]
        public async Task<ActionResult<RegistrationReadDTO>> Register(int eventId, [FromBody] RegistrationWriteDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { message = "User not logged in" });

            var ev = await _eventRepo.GetEventByIDAsync(eventId);

            if (ev == null)
                return NotFound(new { message = "Event not found" });

            if (ev.Date < DateTime.Now)
                return BadRequest(new { message = "Event already finished" });

            if (ev.SeatsAvailable <= 0)
                return BadRequest(new { message = "No seats available" });

            ev.SeatsAvailable--;

            var reg = _mapper.Map<Registration>(dto);

            reg.RegistrationDate = DateTime.Now;

            // link registration to logged-in user
            // we know who made the booking
            reg.UserID = user.UserID;

            reg.EventID = eventId; // link registration to the selected event from URL

            await _regRepo.CreateAsync(reg);
            await _eventRepo.UpdateAsync(ev);

            return Ok(_mapper.Map<RegistrationReadDTO>(reg));
        }

        // GET all registrations
        [HttpGet]
        public async Task<ActionResult<List<RegistrationReadDTO>>> GetAll()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { message = "User not logged in" });

            var regs = await _regRepo.GetRegistrationsAsync();

            // ROLE FILTERING
            if (user.Role == "Public")
            {
                regs = regs.Where(r => r.UserID == user.UserID).ToList();
            }
            else if (user.Role == "Organizer" && user.Organizer != null)
            {
                regs = regs.Where(r => r.Event != null && r.Event.OrganizerID == user.Organizer.OrganizerID).ToList();
            }
            // Admin sees all

            regs = regs.OrderByDescending(r => r.RegistrationDate).ToList();

            return Ok(_mapper.Map<List<RegistrationReadDTO>>(regs));
        }

        // GET registration by ID (organizer can only see their own)
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistrationReadDTO>> GetById(int id)
        {
            var reg = await _regRepo.GetRegistrationByIDAsync(id);
            if (reg == null)
                return NotFound(new { message = "Registration not found" });

            var user = await GetCurrentUserAsync();
            if (user != null && user.Role == "Organizer" && user.Organizer != null)
            {
                if (reg.Event?.OrganizerID != user.Organizer.OrganizerID)
                    return Unauthorized(new { message = "You can only see registrations for your events" });
            }

            return Ok(_mapper.Map<RegistrationReadDTO>(reg));
        }

        // SEARCH registrations
        [HttpGet("search")]
        public async Task<ActionResult<List<RegistrationReadDTO>>> Search(
            string? key,
            string? participantName,
            string? participantEmail,
            int? registrationId,
            int? eventId,
            int? userId,
            int? organizerId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { message = "User not logged in" });

            var regs = await _regRepo.GetRegistrationsAsync();

            // ROLE FILTERING FIRST
            if (user.Role == "Public")
            {
                regs = regs.Where(r => r.UserID == user.UserID).ToList();
            }
            else if (user.Role == "Organizer" && user.Organizer != null)
            {
                regs = regs.Where(r => r.Event != null && r.Event.OrganizerID == user.Organizer.OrganizerID).ToList();
            }
            // Admin sees all

            // KEYWORD SEARCH (LIKE YOUR EVENTS)
            if (!string.IsNullOrEmpty(key))
            {
                var lowerKey = key.ToLower();

                regs = regs.Where(r =>
                    (r.ParticipantName != null && r.ParticipantName.ToLower().Contains(lowerKey)) ||
                    (r.ParticipantEmail != null && r.ParticipantEmail.ToLower().Contains(lowerKey)) ||
                    (r.Event != null && r.Event.Title != null && r.Event.Title.ToLower().Contains(lowerKey)) ||
                    r.RegistrationID.ToString().Contains(key) ||
                    r.EventID.ToString().Contains(key) ||
                    (r.UserID != null && r.UserID.ToString().Contains(key)) ||
                    (r.Event != null && r.Event.OrganizerID.ToString().Contains(key))
                ).ToList();
            }

            // SPECIFIC FILTERS

            if (!string.IsNullOrEmpty(participantName))
                regs = regs.Where(r =>
                    r.ParticipantName != null &&
                    r.ParticipantName.ToLower().Contains(participantName.ToLower())
                ).ToList();

            if (!string.IsNullOrEmpty(participantEmail))
                regs = regs.Where(r =>
                    r.ParticipantEmail != null &&
                    r.ParticipantEmail.ToLower().Contains(participantEmail.ToLower())
                ).ToList();

            if (registrationId.HasValue)
                regs = regs.Where(r => r.RegistrationID == registrationId.Value).ToList();

            if (eventId.HasValue)
                regs = regs.Where(r => r.EventID == eventId.Value).ToList();

            if (userId.HasValue)
                regs = regs.Where(r => r.UserID == userId.Value).ToList();

            if (organizerId.HasValue)
                regs = regs.Where(r =>
                    r.Event != null && r.Event.OrganizerID == organizerId.Value
                ).ToList();

            // SORT
            regs = regs.OrderByDescending(r => r.RegistrationDate).ToList();

            return Ok(_mapper.Map<List<RegistrationReadDTO>>(regs));
        }

    }
}