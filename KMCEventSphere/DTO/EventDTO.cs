using System;
using System.ComponentModel.DataAnnotations;

namespace KMCEventSphere.DTOs
{
    public class EventReadDTO
    {
        public int EventID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int SeatsAvailable { get; set; }
        public int OrganizerID { get; set; }
        public string? OrganizerName { get; set; }
        public int RegistrationCount { get; set; }

        // Register Link
        public bool CanRegister { get; set; }
    }

    public class EventWriteDTO
    {
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; } = string.Empty;

        [Range(0, 100000, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "SeatsAvailable is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "SeatsAvailable must be at least 1.")]
        public int SeatsAvailable { get; set; }

    }
}