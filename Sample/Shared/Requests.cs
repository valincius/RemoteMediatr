using Microsoft.AspNetCore.Authorization;
using RemoteMediatr.Core;

namespace Sample.Shared
{
    public class Requests
    {
        public record HelloWorldQuery(string Name) : IClientRequest<string>;

        [Authorize]
        public record WeatherForecastQuery() : IClientRequest<IEnumerable<WeatherForecast>>;

        [Authorize]
        public record ConsoleLogCommand(string Text) : IClientRequest;
    }
}
