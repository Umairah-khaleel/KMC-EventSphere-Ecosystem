using KMCEventSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace KMCEventSphere.Data
{
    public class RegistrationRepo
    {

        private readonly KMCEventSphereDBContext _db;

        public RegistrationRepo(KMCEventSphereDBContext db)
        {
            _db = db;
        }

        // Save changes to DB
        public async Task<bool> SaveAsync() => await _db.SaveChangesAsync() > 0;

        // Create a new registration
        public async Task<bool> CreateAsync(Registration reg)
        {
            if (reg != null)
            {
                await _db.Registrations.AddAsync(reg);
                return await SaveAsync();
            }
            return false;
        }

        // Update an existing registration
        public async Task<bool> UpdateAsync(Registration reg)
        {
            if (reg != null)
            {
                _db.Registrations.Update(reg);
                return await SaveAsync();
            }
            return false;
        }

        // Remove a registration
        public async Task<bool> RemoveAsync(Registration reg)
        {
            if (reg != null)
            {
                _db.Registrations.Remove(reg);
                return await SaveAsync();
            }
            return false;
        }

        // Get all registrations (include Event for OrganizerID)
        public async Task<List<Registration>> GetRegistrationsAsync() =>
            await _db.Registrations.Include(r => r.Event).ToListAsync();

        // Get registration by ID (nullable, include Event)
        public async Task<Registration?> GetRegistrationByIDAsync(int id) =>
            await _db.Registrations.Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RegistrationID == id);

        // Get registrations for specific organizer
        public async Task<List<Registration>> GetRegistrationsByOrganizerAsync(int organizerId) =>
            await _db.Registrations
                .Include(r => r.Event)
                .Where(r => r.Event != null && r.Event.OrganizerID == organizerId)
                .ToListAsync();
        
    }

}