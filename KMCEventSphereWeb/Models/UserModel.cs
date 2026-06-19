using System.ComponentModel.DataAnnotations;

namespace KMCEventSphereWeb.Models
{
    public class UserModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;


        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = "Public"; // default role

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty; // only for creating users


    }
}