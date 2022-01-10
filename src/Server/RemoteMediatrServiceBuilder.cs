using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
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

            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(ctx.Request.ContentType), 16384);
            var reader = new MultipartReader(boundary, ctx.Request.Body);
            var section = await reader.ReadNextSectionAsync();
            
            var body = await section!.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize(body, type);
            if (obj is null)
                return Results.BadRequest($"Could not convert body to {requestType}");

            Dictionary<string, Stream> uploadedFiles = new();
            section = await reader.ReadNextSectionAsync();
            while (section is not null)
            {
                var file = section.AsFileSection();
                if (file?.FileStream is not null)
                {
                    var streamCopy = new MemoryStream();
                    file.FileStream.CopyTo(streamCopy);
                    streamCopy.Seek(0, SeekOrigin.Begin);
                    uploadedFiles.Add(file.Name, streamCopy);
                }

                section = await reader.ReadNextSectionAsync();
            }

            var streamProperties = type.GetProperties().Where(x => x.PropertyType.IsAssignableFrom(typeof(Stream)));
            foreach (var prop in streamProperties)
            {
                // throw if stream is non-nullable but is null
                if (uploadedFiles.TryGetValue(prop.Name, out var stream))
                    prop.SetValue(obj, stream);
            }

            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService(typeof(IMediator)) as IMediator;
            var result = await mediator!.Send(obj);

            foreach (var file in uploadedFiles)
                file.Value.Dispose();

            return Results.Ok(result);
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
