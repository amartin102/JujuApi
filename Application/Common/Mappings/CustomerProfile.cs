using Application.Dtos.Customer;
using Application.Dtos.Post;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {          

            CreateMap<CreatePostDto, PostEntity>().ReverseMap();
            CreateMap<CustomerEntity, GetCustomerDto>().ReverseMap();

            CreateMap<PostEntity, GetPostDto>()
                .ForMember(dest => dest.CustomerName,
                           opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null));

            CreateMap<UpdatePostDto, PostEntity>()
               .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
               .ReverseMap();
        }
    }
}
