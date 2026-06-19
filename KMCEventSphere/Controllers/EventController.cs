using AutoMapper;
using KMCEventSphere.Data;
using KMCEventSphere.DTOs;
using KMCEventSphere.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KMCEventSphere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventRepo _repo;
        private readonly UserRepo _userRepo;
        private readonly IMapper _mapper;

        public EventController(EventRepo repo, UserRepo userRepo, IMapper mapper)
        {
            _repo = repo;
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

        // GET all events

        [HttpGet]
        public async Task<ActionResult<List<EventReadDTO>>> GetAll()
        {
            var user = await GetCurrentUserAsync();

            List<Event> events;

            if (user == null)
            {
                // PUBLIC USER (not logged in) //added this
                events = (await _repo.GetEventsAsync())
                          .OrderByDescending(e => e.EventID)
                          .ToList();
            }

            else if (user.Role == "Organizer" && user.Organizer != null)
            {
                // Organizer sees only their own events
                events = (await _repo.GetEventsAsync())
                          .Where(e => e.OrganizerID == user.Organizer.OrganizerID)
                          .OrderByDescending(e => e.EventID) // latest events first
                          .ToList();
            }
            else
            {
                // Admin & Public sees all events
                events = (await _repo.GetEventsAsync())
                          .OrderByDescending(e => e.EventID) // latest events first
                          .ToList();
            }

            // add registration count
            var result = _mapper.Map<List<EventReadDTO>>(events);

            foreach (var e in result)
            {
                e.RegistrationCount = await _repo.GetRegistrationCountAsync(e.EventID);
            }

            return Ok(result);
        }
        

        // GET event by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<EventReadDTO>> GetById(int id)
        {
            var ev = await _repo.GetEventByIDAsync(id);
            if (ev == null)
                return NotFound(new { message = "Event not found" });

            return Ok(_mapper.Map<EventReadDTO>(ev));
        }

        // CREATE event
        [HttpPost]
        public async Task<ActionResult<EventReadDTO>> Create([FromBody] EventWriteDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get current logged-in user
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { message = "User not logged in" });

            // Only Organizers can create events
            if (user.Role != "Organizer")
                return Unauthorized(new { message = "Only organizers can create events" });

            // Make sure the user is an Organizer
            if (user.Organizer == null)
                return Unauthorized(new { message = "User is not registered as an organizer" });

            // Map DTO to Event entity
            var ev = _mapper.Map<Event>(dto);

            // Safe linking: assign OrganizerID
            ev.OrganizerID = user.Organizer.OrganizerID;

            // Safe linking: assign UserID
            ev.UserID = user.UserID;


            var success = await _repo.CreateAsync(ev);
            if (!success)
                return StatusCode(500, new { message = "Failed to create event" });

            return Ok(_mapper.Map<EventReadDTO>(ev));
        }

        // UPDATE event
        [HttpPut("{id}")]
        public async Task<ActionResult<EventReadDTO>> Update(int id, [FromBody] EventUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { message = "User not logged in" });

            var ev = await _repo.GetEventByIDAsync(id);
            if (ev == null)
                return NotFound(new { message = "Event not found" });

            // Only organizers allowed
            if (user.Role != "Organizer")
                return Unauthorized(new { message = "Only organizers can update events" });

            // Organizer can update only their own events
            if (user.Role == "Organizer" && (user.Organizer == null || ev.OrganizerID != user.Organizer.OrganizerID))
                return Unauthorized(new { message = "You can only update your own events" });

            if (dto.Date.HasValue)
                ev.Date = dto.Date.Value;

            if (dto.Location != null)
                ev.Location = dto.Location;

            if (dto.SeatsAvailable.HasValue)
            {
                int registeredCount = await _repo.GetRegistrationCountAsync(id);

                if (dto.SeatsAvailable.Value < registeredCount)
                {
                    return BadRequest(new { message = "Seats cannot be less than registered users" });
                }

                ev.SeatsAvailable = dto.SeatsAvailable.Value;
            }

            await _repo.UpdateAsync(ev);

            return Ok(_mapper.Map<EventReadDTO>(ev));
        }
        // DELETE event
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { message = "User not logged in" });

            var ev = await _repo.GetEventByIDAsync(id);
            if (ev == null)
                return NotFound(new { message = "Event not found" });

            // Organizer can delete only their own events
            if (user.Role == "Organizer" && (user.Organizer == null || ev.OrganizerID != user.Organizer.OrganizerID))
                return Unauthorized(new { message = "You can only delete your own events" });

            if (user.Role != "Organizer")
                return Unauthorized(new { message = "Only organizers can delete events" });

            int registeredCount = await _repo.GetRegistrationCountAsync(id);

            if (registeredCount > 0)
            {
                return BadRequest(new { message = "Cannot delete event with registrations." });
            }


            await _repo.RemoveAsync(ev);
            return Ok(new { message = "Event deleted successfully" });
        }

        // SEARCH 
        [HttpGet("search")]
        public async Task<ActionResult<List<EventReadDTO>>> Search(
              string? key,
              string? title,
              string? category,
              string? location,
              string? organizerName,
              DateTime? date,
              int? eventId,
              int? organizerId,
              decimal? price)
        {
            var user = await GetCurrentUserAsync();

            var events = await _repo.GetEventsAsync();

            if (user != null && user.Role == "Organizer" && user.Organizer != null)
            {
                events = events.Where(e => e.OrganizerID == user.Organizer.OrganizerID).ToList();
            }

            // KEYWORD SEARCH (search EVERYTHING)
            if (!string.IsNullOrEmpty(key))
            {
                var lowerKey = key.ToLower();

                events = events.Where(e =>
                    (e.Title != null && e.Title.ToLower().Contains(lowerKey)) ||
                    (e.Category != null && e.Category.ToLower().Contains(lowerKey)) ||
                    (e.Location != null && e.Location.ToLower().Contains(lowerKey)) ||
                    (e.Description != null && e.Description.ToLower().Contains(lowerKey)) ||
                    (e.Organizer != null && e.Organizer.OrganizationName != null &&
                     e.Organizer.OrganizationName.ToLower().Contains(lowerKey)) ||
                    e.EventID.ToString().Contains(key) ||
                    e.Price.ToString().Contains(key) ||
                    e.Date.ToString().Contains(key)
                ).ToList();
            }

            // SPECIFIC FILTERS

            if (!string.IsNullOrEmpty(title))
                events = events.Where(e =>
                    e.Title != null &&
                    e.Title.ToLower().Contains(title.ToLower())
                ).ToList();

            if (!string.IsNullOrEmpty(category))
                events = events.Where(e =>
                    e.Category != null &&
                    e.Category.ToLower().Contains(category.ToLower())
                ).ToList();

            if (!string.IsNullOrEmpty(location))
                events = events.Where(e =>
                    e.Location != null &&
                    e.Location.ToLower().Contains(location.ToLower())
                ).ToList();

            if (!string.IsNullOrEmpty(organizerName))
                events = events.Where(e =>
                    e.Organizer != null &&
                    e.Organizer.OrganizationName != null &&
                    e.Organizer.OrganizationName.ToLower().Contains(organizerName.ToLower())
                ).ToList();

            // DATE (ignore time completely)
            if (date.HasValue)
                events = events.Where(e =>
                    e.Date.Date == date.Value.Date
                ).ToList();

            // KEEP EXACT MATCH (DO NOT CHANGE)
            if (eventId.HasValue)
                events = events.Where(e => e.EventID == eventId.Value).ToList();

            if (price.HasValue)
                events = events.Where(e => e.Price == price.Value).ToList();

            if (organizerId.HasValue)
                events = events.Where(e => e.OrganizerID == organizerId.Value).ToList();

            events = events.OrderByDescending(e => e.EventID).ToList(); //Latest Event Comes At The Top 

            return Ok(_mapper.Map<List<EventReadDTO>>(events));
        }
    }
}