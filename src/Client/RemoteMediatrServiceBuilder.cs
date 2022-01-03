using Microsoft.Extensions.DependencyInjection;
using RemoteMediatr.Core;

namespace RemoteMediatr.Client;

public static class RemoteMediatrServiceBuilder
{
    public static IServiceCollection AddRemoteMediatrClient(this IServiceCollection services, Action<IHttpClientBuilder>? httpClientBuilder = null)
    {
        var httpClient = services.AddHttpClient(Constants.HttpClientName);
        httpClientBuilder?.Invoke(httpClient);

        services.AddSingleton<IRemoteMediatr, RemoteMediatr>();
        return services;
    }
}
