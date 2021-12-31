using MediatR;

namespace DemoApp.Shared
{
    public class Requests
    {
        public record HelloWorldQuery(string Name) : IRequest<string>;

        public record WeatherForecastQuery() : IRequest<IEnumerable<WeatherForecast>>;
    }
}
