using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using Microsoft.CodeAnalysis.CodeActions;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(dto => dto.Age, config => config.MapFrom(entity => entity.DateOfBirth.CalculateAge()))
            .ForMember(dto => dto.PhotoUrl, config => config.MapFrom(entity => entity.Photos.FirstOrDefault(p => p.IsMain)!.Url))
        ;
        CreateMap<Photo, PhotoDto>();

        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
        CreateMap<string, DateOnly>().ConvertUsing(s => DateOnly.Parse(s));
    }

}
