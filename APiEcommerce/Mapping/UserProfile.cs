using System;
using APiEcommerce.Models;
using APiEcommerce.Models.Dtos;
using AutoMapper;

namespace APiEcommerce.Mapping;

public class UserProfile:Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<User, CreateUserDto>().ReverseMap();
        CreateMap<User, UserLoginDto>().ReverseMap();
        CreateMap<User, UserLoginResponseDto>().ReverseMap();
        CreateMap<ApplicationUser,UserDataDto>().ReverseMap();
        CreateMap<ApplicationUser,UserDto>().ReverseMap();
    }
}
