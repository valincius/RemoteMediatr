using Microsoft.AspNetCore.Authorization;
using RemoteMediatr.Core;

namespace DemoApp.Shared
{
    public class Requests
    {
        [Authorize]
        public record HelloWorldQuery(string Name) : IClientRequest<string>;

        [Authorize(Policy = "weather:read")]
        public record WeatherForecastQuery() : IClientRequest<IEnumerable<WeatherForecast>>;
    }
}
