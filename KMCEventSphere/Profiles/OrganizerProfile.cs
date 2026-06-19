using AutoMapper;
using KMCEventSphere.DTOs;
using KMCEventSphere.Models;

namespace KMCEventSphere.Profiles
{
    public class OrganizerProfile : Profile
    {
        public OrganizerProfile()
        {
            // Original mapping from write DTO to Organizer entity
            CreateMap<OrganizerWriteDTO, Organizer>();

            // Mapping from Organizer entity to read DTO (safe handling for null User)
            CreateMap<Organizer, OrganizerReadDTO>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.User != null ? src.User.UserID : 0))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : null))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.OrganizationName))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // Admin creation mapping: AdminCreateOrganizerDTO → User
            CreateMap<AdminCreateOrganizerDTO, User>();

            // Admin creation mapping: AdminCreateOrganizerDTO → Organizer (extra fields)
            CreateMap<AdminCreateOrganizerDTO, Organizer>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.OrganizationName))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        }
    }
}