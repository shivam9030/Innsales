using AutoMapper;
using InnSales.Domain.Entities;

using InnSales.Common.DTO;
namespace InnSales.MappingProfiles
{
    public class NewsMappingProfile : Profile
    {
        public NewsMappingProfile()
        {
            CreateMap<News, NewsDtoV1>().ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); ;
            CreateMap<News, NewsDtoV2>().ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); ;
        }
    }

}