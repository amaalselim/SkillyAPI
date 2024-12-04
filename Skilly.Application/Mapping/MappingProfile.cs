using AutoMapper;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterDTO,User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)).ReverseMap();


            CreateMap<UserProfile, UserProfileDTO>()
            .ForMember(dest => dest.Img, opt => opt.Ignore()); 

            CreateMap<UserProfileDTO, UserProfile>()
                .ForMember(dest => dest.Img, opt => opt.MapFrom(src => src.Img != null ? src.Img.FileName : null));

            CreateMap<ServiceProvider, ServiceProviderDTO>()
            .ForMember(dest => dest.Img, opt => opt.Ignore())
            .ForMember(dest => dest.NationalNumberPDF, opt => opt.Ignore());


            CreateMap<ServiceProviderDTO, ServiceProvider>()
                .ForMember(dest => dest.Img, opt => opt.MapFrom(src => src.Img != null ? src.Img.FileName : null))
                .ForMember(dest => dest.NationalNumberPDF, opt => opt.MapFrom(src => src.NationalNumberPDF != null ? src.NationalNumberPDF.FileName : null));
        }

    }
}
