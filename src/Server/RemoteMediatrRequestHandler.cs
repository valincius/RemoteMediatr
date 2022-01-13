using MediatR;
using Microsoft.AspNetCore.Authorization;
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
internal class RemoteMediatrRequestHandler
{
    private readonly Assembly _assembly;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IAuthorizationPolicyProvider? _authorizationPolicyProvider;

    public RemoteMediatrRequestHandler(
        Assembly assembly,
        IServiceScopeFactory serviceScopeFactory,
        IAuthorizationPolicyProvider? authorizationPolicyProvider)
    {
        _assembly = assembly;
        _serviceScopeFactory = serviceScopeFactory;
        _authorizationPolicyProvider = authorizationPolicyProvider;
    }

    public async Task<IResult> HandleRequest(string requestType, HttpContext context)
    {
        var type = (
            from t in _assembly.DefinedTypes
            from i in t.GetInterfaces()
            where t.Name == requestType
            where i.IsGenericType
            where i.GetGenericTypeDefinition() == typeof(IClientRequest<>)
            select t.AsType()
        ).FirstOrDefault();

        if (type is null)
            return Results.BadRequest($"Type {requestType} was not found");

        var authResult = await AuthorizeRequest(context, type);
        if (authResult is not null)
            return Results.Unauthorized();

        using var stream = new StreamReader(context.Request.Body);
        string body = await stream.ReadToEndAsync();
        var obj = JsonSerializer.Deserialize(body, type);

        if (obj is null)
            return Results.BadRequest($"Could not convert payload to {requestType}");

        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService(typeof(IMediator)) as IMediator;
        try
        {
            var result = await mediator!.Send(obj);
            if (result is Unit)
                return Results.NoContent();

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private async Task<IActionResult?> AuthorizeRequest(HttpContext httpContext, Type request)
    {
        if (_authorizationPolicyProvider is null)
            return null;

        var authData = request.GetCustomAttributes<AuthorizeAttribute>();
        if (!authData.Any())
            return null;

        var authorizeFilter = new AuthorizeFilter(_authorizationPolicyProvider, authData);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var authorizationFilterContext = new AuthorizationFilterContext(actionContext, new[] { authorizeFilter });

        await authorizeFilter.OnAuthorizationAsync(authorizationFilterContext);

        return authorizationFilterContext.Result;
    }
}
