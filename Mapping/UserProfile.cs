using System;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using AutoMapper;

namespace Inventory_Flow.Mapping;



public class UserProfile : Profile
{

    public UserProfile ()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<User, CreateUserDto>().ReverseMap();
        CreateMap<User, UserLoginDto>().ReverseMap();
        CreateMap<User, UserLoginResponseDto>().ReverseMap();
    }
}
