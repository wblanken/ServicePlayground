using AutoMapper;
using ServicePlayground.Protobuf.Proto;

namespace ServicePlayground.Protobuf.Profiles;

public class ItemProfile : Profile
{
    public ItemProfile()
    {
        CreateMap<Protobuf.Proto.Item, Common.Model.Item>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.ToDecimal()));
        
        CreateMap<Common.Model.Item, Protobuf.Proto.Item>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => DecimalValue.FromDecimal(src.Price)));
    }
}