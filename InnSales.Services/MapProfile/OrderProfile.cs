using AutoMapper;

using InnSales.Domain.Entities;
using InnSales.Common.DTO;

namespace InnSales.MappingProfiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
                .ForMember(dest => dest.IsPromoProduct, opt => opt.MapFrom(src => src.Product.IsPromoProduct));


            CreateMap<Order, OrderDto>()
.ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.DisplayName))
.ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionId))
.ForMember(dest => dest.CardHolderName, opt => opt.MapFrom(src => src.CardHolderName))
.ForMember(dest => dest.MaskedCardNumber, opt => opt.MapFrom(src => src.MaskedCardNumber))
.ForMember(dest => dest.CardExpiryDate, opt => opt.MapFrom(src => src.CardExpiryDate))
.ForMember(dest => dest.Tax, opt => opt.MapFrom(src => src.Tax))
.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
.ForMember(dest => dest.Subtotal, opt => opt.Ignore())
.ForMember(dest => dest.Discount, opt => opt.Ignore());


        }
    }
}
