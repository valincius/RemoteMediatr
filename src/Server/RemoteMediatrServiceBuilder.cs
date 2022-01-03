using MediatR;
using Microsoft.AspNetCore.Builder;
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
        app.MapPost(Constants.RequestPath, HandleRequest(assembly, mediator));
    }

    private static Func<RemoteMediatrRequest, Task<object>> HandleRequest(Assembly assembly, IMediator mediator) =>
        async (req) =>
        {
            var type = assembly.GetType(req.Name);

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
}
