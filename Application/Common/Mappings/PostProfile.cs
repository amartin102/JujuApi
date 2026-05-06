using Application.Dtos.Customer;
using Application.Dtos.Post;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {          
            CreateMap<CreateCustomerDto, CustomerEntity>().ReverseMap();

            CreateMap<UpdateCustomerDto, CustomerEntity>()
               .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
               .ReverseMap();
        }
    }
}
