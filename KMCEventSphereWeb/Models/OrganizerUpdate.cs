public class OrganizerUpdateModel
{
    public int OrganizerID { get; set; }

    // Admin fields
    public string? OrganizationName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }

    // Organizer fields
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
}