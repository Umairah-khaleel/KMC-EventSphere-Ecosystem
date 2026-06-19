using AutoMapper;
using KMCEventSphere.DTOs;
using KMCEventSphere.Models;

namespace KMCEventSphere.Profiles
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            // Map from DTO to entity
            CreateMap<EventWriteDTO, Event>();

            // Map from entity to DTO, compute CanRegister
            CreateMap<Event, EventReadDTO>()
                .ForMember(dest => dest.CanRegister, opt => opt.MapFrom(src =>
                    src.SeatsAvailable > 0 && src.Date > DateTime.Now
                ))

                .ForMember(dest => dest.OrganizerName, opt => opt.MapFrom(src =>
                    src.Organizer != null ? src.Organizer.OrganizationName : null));
        }
    }
}