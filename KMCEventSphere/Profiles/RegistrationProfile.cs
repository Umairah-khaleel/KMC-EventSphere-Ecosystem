using AutoMapper;
using KMCEventSphere.Models;
using KMCEventSphere.DTOs;

namespace KMCEventSphere.Profiles
{
    public class RegistrationProfile : Profile
    {
        public RegistrationProfile()
        {
            // Write DTO → Registration entity
            CreateMap<RegistrationWriteDTO, Registration>();

            // Registration entity → Read DTO (null-safe OrganizerID)
            CreateMap<Registration, RegistrationReadDTO>()
              .ForMember(dest => dest.OrganizerID, opt => opt.MapFrom(src => src.Event != null ? src.Event.OrganizerID : (int?)null));
        }
    }
}