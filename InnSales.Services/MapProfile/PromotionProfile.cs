
using AutoMapper;
using InnSales.Domain.Entities;
using InnSales.Common.Enums;
using InnSales.Common.DTO;

public class PromotionProfile : Profile
{
    public PromotionProfile()
    {
        // Map Create DTO → Entity
        CreateMap<PromotionCreateDto, Promotion>()
            .ForMember(dest => dest.PromotionId, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Map Update DTO → Entity
        CreateMap<PromotionUpdateDto, Promotion>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Map Entity → Response DTO
        CreateMap<Promotion, PromotionResponseDto>();

        // PromoCode mappings
        CreateMap<PromoCodeCreateDto, PromoCode>()
            .ForMember(dest => dest.PromoCodeId, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<PromoCode, PromoCodeResponseDto>();
    }
}
