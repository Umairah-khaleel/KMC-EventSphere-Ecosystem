using System.ComponentModel.DataAnnotations;

namespace KMCEventSphereWeb.Models
{
    public class EventModel
    {
        public int EventID { get; set; }
        public int OrganizerID { get; set; }
        public string? OrganizerName { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Time is required")]
        public TimeSpan? Time { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; } = string.Empty;


        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        // Always 1 (hidden)
        public int SeatsAvailable { get; set; } = 1;

        // Registration count 
        public int RegistrationCount { get; set; }

        // Helper property to control registration button
        public bool CanRegister => SeatsAvailable > 0 && Date > DateTime.Now;
    }
}