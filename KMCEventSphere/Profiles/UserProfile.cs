using AutoMapper;
using KMCEventSphere.Models;
using KMCEventSphere.DTOs;

namespace KMCEventSphere.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserWriteDTO, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<User, UserReadDTO>();
        }
    }
}