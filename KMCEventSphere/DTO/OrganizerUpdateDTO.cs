using System.ComponentModel.DataAnnotations;

public class OrganizerUpdateDTO
{
    // Admin-only fields
    public string? OrganizationName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
    public string? FullName { get; set; }

    // Organizer fields

    [RegularExpression(@"^\d{9,15}$", ErrorMessage = "Phone must be 9–15 digits")]
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
}