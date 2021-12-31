using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;
using Valincius.RemoteMediatr.Core;

namespace Valincius.RemoteMediatr.Server;
public static class RemoteMediatrServiceBuilder
{
    public static void MapRemoteMediatrListener(this WebApplication app, Assembly assembly)
    {
        var mediator = app.Services.GetRequiredService<IMediator>();
        app.MapPost(Constants.RequestPath, async (RemoteMediatrRequest req) =>
        {
            var type = assembly.GetType(req.Name);
            var returnType = assembly.GetType(req.ReturnType);
            var obj = JsonSerializer.Deserialize(req.Request, type!);
            var result = await mediator.Send(obj!);
            return JsonSerializer.Serialize(result);
        });
    }
}
