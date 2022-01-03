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
        var mediator = app.Services.GetRequiredService<IMediator>();
        var policyProvider = app.Services.GetService<IAuthorizationPolicyProvider>();

        app.MapPost(Constants.RequestPath, HandleRequest(assembly, mediator, policyProvider));
    }

    private static Func<RemoteMediatrRequest, HttpContext, Task<IResult>> HandleRequest(Assembly assembly, IMediator mediator, IAuthorizationPolicyProvider? policyProvider) =>
        async (req, ctx) =>
        {
            var type = (from t in assembly.DefinedTypes
                        from i in t.GetInterfaces()
                        where t.Name == req.Name
                        where i.IsGenericType
                        where i.GetGenericTypeDefinition() == typeof(IClientRequest<>)
                        select t.AsType())
                        .FirstOrDefault();

            if (type is null)
                return Results.BadRequest($"Type {req.Name} was not found");

            if (policyProvider is not null)
            {
                var authResult = await AuthorizeRequest(policyProvider, ctx, type);
                if (authResult is not null)
                    return Results.Unauthorized();
            }

            var obj = JsonSerializer.Deserialize(req.Request, type);
            if (obj is null)
                return Results.BadRequest($"Could not convert payload to {type.Name}");

            return Results.Ok(await mediator.Send(obj));
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
