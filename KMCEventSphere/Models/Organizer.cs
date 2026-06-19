using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KMCEventSphere.Models
{
    public class Organizer
    {
        [Key]
        public int OrganizerID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        // New fields
        public string? OrganizationName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }

        public User? User { get; set; }
        public List<Event> Events { get; set; } = new List<Event>();
    }
}