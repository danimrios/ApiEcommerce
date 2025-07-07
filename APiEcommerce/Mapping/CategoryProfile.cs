using System;
using APiEcommerce.Models.Dtos;
using AutoMapper;
namespace APiEcommerce.Mapping;

public class CategoryProfile:Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Category, CreateCategoryDto>().ReverseMap();
    }
}
