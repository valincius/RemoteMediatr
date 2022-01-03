using Valincius.RemoteMediatr.Core;

namespace DemoApp.Shared
{
    public class Requests
    {
        public record HelloWorldQuery(string Name) : IClientRequest<string>;

        public record WeatherForecastQuery() : IClientRequest<IEnumerable<WeatherForecast>>;
    }
}
