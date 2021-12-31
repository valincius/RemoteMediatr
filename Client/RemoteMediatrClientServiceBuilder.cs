using Microsoft.Extensions.DependencyInjection;
using Valincius.RemoteMediatr.Core;

namespace Valincius.RemoteMediatr.Client;

public static class RemoteMediatrClientServiceBuilder
{
    public static IServiceCollection AddRemoteMediatr(this IServiceCollection services, Action<HttpClient> httpClientOptions)
    {
        services.AddHttpClient(Constants.HttpClientName, httpClientOptions);
        services.AddSingleton<RemoteMediatrSender>();
        return services;
    }
}
