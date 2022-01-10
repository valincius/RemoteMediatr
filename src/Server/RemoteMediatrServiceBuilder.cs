using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RemoteMediatr.Core;
using System.Reflection;
using System.Text.Json;

namespace RemoteMediatr.Server;

public static class RemoteMediatrServiceBuilder
{
    public static void MapRemoteMediatrListener(this WebApplication app, Assembly assembly)
    {
        var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
        var policyProvider = app.Services.GetService<IAuthorizationPolicyProvider>();
        
        app.MapPost($"{Constants.RequestPath}/{{requestType}}", HandleRequest(assembly, scopeFactory, policyProvider));
    }

    private static Func<string, HttpContext, Task<IResult>> HandleRequest(Assembly assembly, IServiceScopeFactory scopeFactory, IAuthorizationPolicyProvider? policyProvider) =>
        async (requestType, ctx) =>
        {
            var type = (from t in assembly.DefinedTypes
                        from i in t.GetInterfaces()
                        where t.Name == requestType
                        where i.IsGenericType
                        where i.GetGenericTypeDefinition() == typeof(IClientRequest<>)
                        select t.AsType())
                        .FirstOrDefault();

            if (type is null)
                return Results.BadRequest($"Type {requestType} was not found");

            if (policyProvider is not null)
            {
                var authResult = await AuthorizeRequest(policyProvider, ctx, type);
                if (authResult is not null)
                    return Results.Unauthorized();
            }

            using var stream = new StreamReader(ctx.Request.Body);
            string body = await stream.ReadToEndAsync();
            var obj = JsonSerializer.Deserialize(body, type);

            if (obj is null)
                return Results.BadRequest($"Could not convert payload to {requestType}");

            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService(typeof(IMediator)) as IMediator;
            return Results.Ok(await mediator!.Send(obj));
        };

    private static async Task<IActionResult?> AuthorizeRequest(IAuthorizationPolicyProvider policyProvider, HttpContext httpContext, Type request)
    {
        var authData = request.GetCustomAttributes<AuthorizeAttribute>();
        if (!authData.Any())
            return null;

        var authorizeFilter = new AuthorizeFilter(policyProvider, authData);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var authorizationFilterContext = new AuthorizationFilterContext(actionContext, new[] { authorizeFilter });

        await authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        return authorizationFilterContext.Result;
    }
}
