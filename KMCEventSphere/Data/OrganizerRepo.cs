using KMCEventSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace KMCEventSphere.Data
{
    public class OrganizerRepo
    {
        private readonly KMCEventSphereDBContext _db;

        public OrganizerRepo(KMCEventSphereDBContext db)
        {
            _db = db;
        }

        // Save changes to DB asynchronously
        public async Task<bool> SaveAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }

        // Create a new organizer
        public async Task<bool> CreateAsync(Organizer org)
        {
            if (org == null) return false;

            await _db.Organizers.AddAsync(org);
            return await SaveAsync();
        }

        // Update an existing organizer
        public async Task<bool> UpdateAsync(Organizer org)
        {
            if (org == null) return false;

            _db.Organizers.Update(org);
            return await SaveAsync();
        }

        // Remove an organizer
        public async Task<bool> RemoveAsync(Organizer org)
        {
            if (org == null) return false;

            _db.Organizers.Remove(org);
            return await SaveAsync();
        }

        // Get all organizers
        public async Task<List<Organizer>> GetOrganizersAsync()
        {
            return await _db.Organizers
                .Include(o => o.User) // include linked user
                .ToListAsync();
        }

        // Get organizer by ID (nullable)
        public async Task<Organizer?> GetOrganizerByIDAsync(int id)
        {
            return await _db.Organizers
                .Include(o => o.User) // include linked user
                .FirstOrDefaultAsync(o => o.OrganizerID == id);
        }

        // Search organizers by name
        public async Task<List<Organizer>> SearchByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<Organizer>();

            return await _db.Organizers
                .Include(o => o.User)
                .Where(o => o.Name != null && o.Name.Contains(name))
                .ToListAsync();
        }
    }
}