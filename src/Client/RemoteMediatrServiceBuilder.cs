using Microsoft.Extensions.DependencyInjection;
using RemoteMediatr.Core;

namespace RemoteMediatr.Client;

public static class RemoteMediatrServiceBuilder
{
    public static IServiceCollection AddRemoteMediatrClient(this IServiceCollection services, Action<HttpClient> httpClientOptions)
    {
        services.AddHttpClient(Constants.HttpClientName, httpClientOptions);
        services.AddSingleton<IRemoteMediatr, RemoteMediatr>();
        return services;
    }
}
