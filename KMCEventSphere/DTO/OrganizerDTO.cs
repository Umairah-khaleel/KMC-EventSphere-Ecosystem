namespace KMCEventSphere.DTOs
{
    public class OrganizerReadDTO
    {
        public int OrganizerID { get; set; }
        public int UserID { get; set; }              
        public string? Username { get; set; }        
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? OrganizationName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
    }
    public class OrganizerWriteDTO
    {
        public int UserID { get; set; }
        public string? Phone { get; set; }
        public string? OrganizationName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
    }
}