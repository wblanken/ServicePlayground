﻿using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ServicePlayground.Protobuf.Client;

public abstract class ClientBase
{
    public ILogger<ClientBase> Logger { get; }
    protected IMapper Mapper { get; }
    protected string ServiceUrl { get; }

    protected ClientBase(ILogger<ClientBase> logger, IMapper mapper, IConfiguration configuration, string serviceName)
    {
        Logger = logger;
        Mapper = mapper;
        if(configuration.GetSection("WPF_USE_SERVER")?.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false)
        {
            ServiceUrl = configuration.GetRequiredSection("Services")[$"{serviceName}:devUrl"];
        }
        else
        {
            ServiceUrl = configuration.GetRequiredSection("Services")[$"{serviceName}:url"];
        }
    }
}