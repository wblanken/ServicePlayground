using AutoMapper;
using ServicePlayground.Common.Proto;

namespace ServicePlayground.GrpcService.Profiles;

public class ItemProfile : Profile
{
    public ItemProfile()
    {
        CreateMap<Common.Proto.Item, Common.Model.Item>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.ToDecimal()));
        
        CreateMap<Common.Model.Item, Common.Proto.Item>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => DecimalValue.FromDecimal(src.Price)));
    }
}