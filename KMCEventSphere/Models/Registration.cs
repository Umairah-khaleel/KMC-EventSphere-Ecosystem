using System.ComponentModel.DataAnnotations;

namespace KMCEventSphere.Models
{
    public class Registration
    {
        [Key]
        public int RegistrationID { get; set; }

        public int EventID { get; set; }

        public string? ParticipantName { get; set; }

        public string? ParticipantEmail { get; set; }

        public DateTime RegistrationDate { get; set; }

        //  user link
        public int? UserID { get; set; }
        public User? User { get; set; }

        public Event? Event { get; set; }
    }
}