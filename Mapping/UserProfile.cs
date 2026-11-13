using System;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using AutoMapper;

namespace Inventory_Flow.Mapping;



public class UserProfile : Profile
{

    public UserProfile ()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty));
        CreateMap<User, CreateUserDto>().ReverseMap();
        CreateMap<User, UserLoginDto>().ReverseMap();
        CreateMap<User, UserLoginResponseDto>().ReverseMap();
    }
}
