using AutoMapper;
using InnSales.Domain.Entities;
using InnSales.Common.DTO;

namespace InnSales.MappingProfiles
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<Payment, PaymentDto>().ReverseMap()
                .ForMember(dest => dest.MaskedCardNumber, opt => opt.Ignore())      // Masking handled in service
                .ForMember(dest => dest.TransactionId, opt => opt.Ignore())        // Generated in service
                .ForMember(dest => dest.TransactionTime, opt => opt.Ignore())      // Generated in service
                .ForMember(dest => dest.PaymentDate, opt => opt.Ignore())          // Set in service
                .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())        // Set in service
                .ForMember(dest => dest.ReadableOrderId, opt => opt.Ignore());     // Set from order
        }
    }
}
