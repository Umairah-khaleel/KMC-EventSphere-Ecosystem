using KMCEventSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace KMCEventSphere.Data
{
    public class EventRepo
    {
        private KMCEventSphereDBContext _db;

        public EventRepo(KMCEventSphereDBContext db)
        {
            _db = db;
        }


        // async versions
        public async Task<bool> SaveAsync() => (await _db.SaveChangesAsync()) > 0;

        public async Task<bool> CreateAsync(Event ev)
        {
            if (ev != null)
            {
                await _db.Events.AddAsync(ev);
                return await SaveAsync();
            }
            return false;
        }

        public async Task<bool> UpdateAsync(Event ev)
        {
            if (ev != null)
            {
                _db.Events.Update(ev);
                return await SaveAsync();
            }
            return false;
        }

        public async Task<int> GetRegistrationCountAsync(int eventId)
        {
            return await _db.Registrations
                .CountAsync(r => r.EventID == eventId);
        }

        public async Task<bool> RemoveAsync(Event ev)
        {
            if (ev != null)
            {
                _db.Events.Remove(ev);
                return await SaveAsync();
            }
            return false;
        }

        public async Task<List<Event>> GetEventsAsync()
        {
            return await _db.Events
                            .Include(e => e.Organizer)
                            .ToListAsync();
        }

        public async Task<Event?> GetEventByIDAsync(int id)
        {
            return await _db.Events
                            .Include(e => e.Organizer)
                            .FirstOrDefaultAsync(e => e.EventID == id);
        }

        public async Task<List<Event>> SearchAsync(string? category, DateTime? date, int? organizerId)
        {
            var query = _db.Events.AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(e => e.Category == category);

            if (date.HasValue)
                query = query.Where(e => e.Date.Date == date.Value.Date);

            if (organizerId.HasValue)
                query = query.Where(e => e.OrganizerID == organizerId);

            return await query.Include(e => e.Organizer).ToListAsync();
        }
    }
}