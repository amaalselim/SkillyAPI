using AutoMapper;
using Microsoft.AspNetCore.Http;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User Register
        CreateMap<RegisterDTO, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ReverseMap();

        // UserProfile
        CreateMap<UserProfile, UserProfileDTO>()
            .ForMember(dest => dest.Img, opt => opt.Ignore());

        CreateMap<UserProfileDTO, UserProfile>()
            .ForMember(dest => dest.Img, opt => opt.Ignore());

        // ServiceProvider
        CreateMap<ServiceProvider, ServiceProviderDTO>()
            .ForMember(dest => dest.Img, opt => opt.Ignore())
            .ForMember(dest => dest.NationalNumberPDF, opt => opt.Ignore());

        CreateMap<ServiceProviderDTO, ServiceProvider>()
            .ForMember(dest => dest.Img, opt => opt.Ignore())
            .ForMember(dest => dest.NationalNumberPDF, opt => opt.Ignore());

        // Service Gallery
        CreateMap<Servicesgallery, servicegalleryDTO>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.galleryImages.Select(img => new ServicesgalleryImage
            {
                Img = img.Img
            }).ToList()));

        CreateMap<servicegalleryDTO, Servicesgallery>()
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        // Provider Services
        CreateMap<ProviderServices, ProviderservicesDTO>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ServicesImages.Select(img => new ProviderServicesImage
            {
                Img = img.Img
            }).ToList()));

        CreateMap<ProviderservicesDTO, ProviderServices>()
            .ForMember(dest => dest.ServicesImages, opt => opt.Ignore());

        // Request Service
        CreateMap<RequestService, requestServiceDTO>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.requestServiceImages.Select(img => new requestServiceImage
            {
                Img = img.Img
            }).ToList()));

        CreateMap<requestServiceDTO, RequestService>()
            .ForMember(dest => dest.requestServiceImages, opt => opt.Ignore());


        CreateMap<ProviderServices, EditProviderServiceDTO>()
    .ForMember(dest => dest.Images, opt => opt.Ignore())
    .ForMember(dest => dest.ImagesToDeleteIds, opt => opt.Ignore())
    .ForMember(dest => dest.video, opt => opt.Ignore());

        CreateMap<EditProviderServiceDTO, ProviderServices>()
            .ForMember(dest => dest.ServicesImages, opt => opt.Ignore())
            .ForMember(dest => dest.video, opt => opt.Ignore());


        CreateMap<EditRequestServiceDTO, RequestService>()
           .ForMember(dest => dest.requestServiceImages, opt => opt.Ignore());

        CreateMap<RequestService, EditRequestServiceDTO>();

        // Review
        CreateMap<Review, ReviewDTO>();
        CreateMap<ReviewDTO, Review>();

        CreateMap<Review, ReviewServiceDTO>();
        CreateMap<ReviewServiceDTO, Review>();

        // Category
        CreateMap<Category, CategoryDTO>()
            .ForMember(dest => dest.Img, opt => opt.Ignore());

        CreateMap<CategoryDTO, Category>()
            .ForMember(dest => dest.Img, opt => opt.Ignore());

        // Banner
        CreateMap<Banner, BannerCreateDTO>()
            .ForMember(dest => dest.Image, opt => opt.Ignore());

        CreateMap<BannerCreateDTO, Banner>()
            .ForMember(dest => dest.ImagePath, opt => opt.Ignore());

        // Offer Salary
        CreateMap<OfferSalary, offersalaryDTO>();

        CreateMap<offersalaryDTO, OfferSalary>()
            .ForMember(dest => dest.serviceId, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.serviceId) ? null : src.serviceId))
            .ForMember(dest => dest.requestserviceId, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.serviceId) ? null : src.serviceId));


        CreateMap<Chat, ChatDTO>()
             .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
             .ForMember(dest => dest.LastUpdatedAt, opt => opt.MapFrom(src => src.LastUpdatedAt));

        CreateMap<Message,MessageResponseDto>()
            .ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => src.SentAt));

        CreateMap<MessageDTO, Message>();
        CreateMap<Message, MessageDTO>();

        CreateMap<CreateChatDTO, Chat>();
        CreateMap<ChatDTO, Chat>(); 


    }
}
