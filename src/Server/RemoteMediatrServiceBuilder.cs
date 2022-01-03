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
        var policyProvider = app.Services.GetRequiredService<IAuthorizationPolicyProvider>(); // make optional
        app.MapPost(Constants.RequestPath, HandleRequest(assembly, mediator, policyProvider));
    }

    private static Func<RemoteMediatrRequest, HttpContext, Task<object>> HandleRequest(Assembly assembly, IMediator mediator, IAuthorizationPolicyProvider policyProvider) =>
        async (req, ctx) =>
        {
            var type = assembly.GetType(req.Name);
            
            var authResult = await AuthorizeRequest(policyProvider, ctx, type!);
            if (authResult is not null)
                return authResult;

            if (type is null)
                throw new InvalidOperationException($"Type {req.Name} was not found");

            var implementsInterface = type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(IClientRequest<>));
            if (!implementsInterface)
                throw new InvalidOperationException($"{type.Name} does not implement {nameof(IClientRequest)}");

            var obj = JsonSerializer.Deserialize(req.Request, type);
            if (obj is null)
                throw new InvalidOperationException($"Could not convert payload to {type.Name}");

            var result = await mediator.Send(obj);
            return JsonSerializer.Serialize(result);
        };

    private static async Task<IActionResult?> AuthorizeRequest(IAuthorizationPolicyProvider policyProvider, HttpContext httpContext, Type request)
    {
        var authData = request.GetCustomAttributes<AuthorizeAttribute>();

        var authorizeFilter = new AuthorizeFilter(policyProvider, authData);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var authorizationFilterContext = new AuthorizationFilterContext(actionContext, new[] { authorizeFilter });

        await authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        return authorizationFilterContext.Result;
    }
}
