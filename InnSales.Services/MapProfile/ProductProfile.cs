using AutoMapper;
using InnSales.Domain.Entities;
using InnSales.Common.DTO;

namespace InnSales.MappingProfiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom(src => src.AvailabilityStatus))
                .ForMember(dest => dest.IsPromoProduct, opt => opt.MapFrom(src => src.IsPromoProduct));;
        }
    }
}