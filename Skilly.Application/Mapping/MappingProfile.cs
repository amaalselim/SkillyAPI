using AutoMapper;
using Microsoft.AspNetCore.Http;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterDTO, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ReverseMap();

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

        CreateMap<servicegalleryDTO, Servicesgallery>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => MapImages(src.Images))); 

        CreateMap<Servicesgallery, servicegalleryDTO>()
            .ForMember(dest => dest.serviceProviderName, opt => opt.MapFrom(src => src.serviceProvider != null ? src.serviceProvider.FirstName + " " + src.serviceProvider.LastName : "NUll"))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.Select(img => new servicegalleryImageDTO
            {
                Img = img.Img
            }).ToList()));

        CreateMap<ProviderservicesDTO,ProviderServices>()
            .ForMember(dest => dest.ServicesImages, opt => opt.MapFrom(src => Map(src.Images)));

        CreateMap<ProviderServices, ProviderservicesDTO>()
            .ForMember(dest => dest.serviceProviderName, opt => opt.MapFrom(src => src.serviceProvider != null ? src.serviceProvider.FirstName + " " + src.serviceProvider.LastName : "NUll"))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ServicesImages.Select(img => new servicegalleryImageDTO
            {
                Img = img.Img
            }).ToList()));

    }

    private List<ServicesgalleryImage> MapImages(IEnumerable<IFormFile> images)
    {
        if (images == null || !images.Any())
            return new List<ServicesgalleryImage>();

        return images.Select(img => new ServicesgalleryImage
        {
            Img = img.FileName 
        }).ToList();
    }
    private List<ProviderServicesImage> Map(IEnumerable<IFormFile> image)
    {
        if (image == null || !image.Any())
            return new List<ProviderServicesImage>();

        return image.Select(img => new ProviderServicesImage
        {
            Img = img.FileName
        }).ToList();
    }
}
