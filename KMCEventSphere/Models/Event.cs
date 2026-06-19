using System.ComponentModel.DataAnnotations;

namespace KMCEventSphere.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        public string? Title { get; set; }

        public string? Category { get; set; }

        public int? OrganizerID { get; set; }

        //  Link to User (Organizer account)
        public int UserID { get; set; }
        public User? User { get; set; }

        public string? Description { get; set; }

        public DateTime Date { get; set; }

        public string? Location { get; set; }

        public decimal Price { get; set; }

        public int SeatsAvailable { get; set; }

        public Organizer? Organizer { get; set; }

        public List<Registration> Registrations { get; set; } = new List<Registration>();
    }
}