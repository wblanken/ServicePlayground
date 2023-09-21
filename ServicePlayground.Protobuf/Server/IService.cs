using AutoMapper;
using Microsoft.Extensions.Logging;
using ServicePlayground.Service;

namespace ServicePlayground.Protobuf.Server;

public interface IService<TModel, TServiceType>
{
    public ILogger<TServiceType> Logger { get; }
    public IDbService<TModel> DbService { get; }
    public IMapper Mapper { get; }

    public static abstract TServiceType ServiceFactory(ILogger<TServiceType> logger, IDbService<TModel> dbService, IMapper mapper);
}