using System.ComponentModel.DataAnnotations;

namespace KMCEventSphere.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Public"; // Default role
        public Organizer? Organizer { get; set; }

        public List<Event> Events { get; set; } = new();
        public List<Registration> Registrations { get; set; } = new();
    }
}