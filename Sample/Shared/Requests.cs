using Microsoft.AspNetCore.Authorization;
using RemoteMediatr.Core;

namespace Sample.Shared
{
    public class Requests
    {
        public record HelloWorldQuery(string Name) : IClientRequest<string>;

        public record WeatherForecastQuery() : IClientRequest<IEnumerable<WeatherForecast>>;

        public record ConsoleLogCommand(string Text) : IClientRequest;
    }
}
