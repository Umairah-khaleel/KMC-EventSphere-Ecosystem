using System.ComponentModel.DataAnnotations;

namespace KMCEventSphereWeb.Models
{
    public class RegistrationModel
    {
        public int RegistrationID { get; set; }
        public int EventID { get; set; }
        public string? EventTitle { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        public string ParticipantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string ParticipantEmail { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public int? OrganizerID { get; set; }
        public int? UserID { get; set; }
    }
}