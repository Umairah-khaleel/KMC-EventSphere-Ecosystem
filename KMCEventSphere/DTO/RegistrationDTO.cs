using System.ComponentModel.DataAnnotations;

namespace KMCEventSphere.DTOs
{
    public class RegistrationReadDTO
    {
        public int RegistrationID { get; set; }
        public int EventID { get; set; }
        public int? UserID { get; set; }
        public string? EventTitle { get; set; }

        [Required]
        public string ParticipantName { get; set; } = string.Empty; 

        [Required]
        [EmailAddress]
        public string ParticipantEmail { get; set; } = string.Empty; 

        public DateTime RegistrationDate { get; set; }

        // Track organizer of the event
        public int? OrganizerID { get; set; }
    }

    public class RegistrationWriteDTO
    {

        [Required(ErrorMessage = "Participant name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string ParticipantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string ParticipantEmail { get; set; } = string.Empty;
    }
}