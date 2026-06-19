using KMCEventSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace KMCEventSphere.Data
{
    public class UserRepo
    {
        private readonly KMCEventSphereDBContext _db;

        public UserRepo(KMCEventSphereDBContext db) => _db = db;

        // Async save
        public async Task<bool> SaveAsync() => await _db.SaveChangesAsync() > 0;

        // Async create
        public async Task<bool> CreateAsync(User user)
        {
            if (user != null)
            {
                await _db.Users.AddAsync(user);
                return await SaveAsync();
            }
            return false;
        }

        // Async update
        public async Task<bool> UpdateAsync(User user)
        {
            if (user != null)
            {
                _db.Users.Update(user);
                return await SaveAsync();
            }
            return false;
        }

        // Async remove
        public async Task<bool> RemoveAsync(User user)
        {
            if (user != null)
            {
                _db.Users.Remove(user);
                return await SaveAsync();
            }
            return false;
        }

        // Async get all users
        public async Task<List<User>> GetUsersAsync() => await _db.Users.ToListAsync();

        // Async get user by ID
        public async Task<User?> GetUserByIDAsync(int id) =>
            await _db.Users
                     .Include(u => u.Organizer)   //  the Organizer navigation property
                     .FirstOrDefaultAsync(u => u.UserID == id);

        // Async get user by username
        public async Task<User?> GetUserByUsernameAsync(string username) =>
            await _db.Users
             .Include(u => u.Organizer)   // include the Organizer navigation property
             .FirstOrDefaultAsync(u => u.Username == username);
    }
}