using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RemoteMediatr.Core;
using System.Reflection;

namespace RemoteMediatr.Server;

public static class RemoteMediatrServiceBuilder
{
    public static void MapRemoteMediatrListener(this WebApplication app, Assembly assembly)
    {
        var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
        var policyProvider = app.Services.GetService<IAuthorizationPolicyProvider>();
        var requestHandler = new RemoteMediatrRequestHandler(assembly, scopeFactory, policyProvider);

        app.MapPost($"{Constants.RequestPath}/{{requestType}}", requestHandler.HandleRequest);
    }
}
