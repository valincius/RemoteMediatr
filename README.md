# RemoteMediatr
[![RemoteMediatr.Client](https://img.shields.io/nuget/v/RemoteMediatr.Client?label=RemoteMediatr.Client)](https://www.nuget.org/packages/RemoteMediatr.Client/)
[![RemoteMediatr.Server](https://img.shields.io/nuget/v/RemoteMediatr.Server?label=RemoteMediatr.Server)](https://www.nuget.org/packages/RemoteMediatr.Server/)


### This project is a work in progress. And will likely recieve many API changes before `v1.0.0` please keep this in mind when using, there will be breaking changes often.

This library is targeted at Blazor WebAssembly and will allow you to send Mediatr requests over http without needing to add any extra boilerplate/controllers.
Requests from your frontend are sent to an endpoint on your server which handles all of the Mediatr logic without any controllers.

## Weather Forecast example
In the default blazor wasm hosted template, there is a weather forecast controller that has a `Get` endpoint that looks like this:
```c#
[HttpGet]
public IEnumerable<WeatherForecast> Get()
{
    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
    {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    })
    .ToArray();
}
```

Then in `FetchData.razor` page we are sending an http request to get our data:
```c#
forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
```


### We can swap this out to use the Mediatr/Request pattern really easily.
First we define our request:
```c#
public record WeatherForecastQuery() : IClientRequest<IEnumerable<WeatherForecast>>;
```
- Here we use `IClientRequest` instead of `IRequest` this is to help distinguish which requests can be called from the client.

Then we setup a handler for this request:
```c#
public class WeatherForecastQueryHandler : IRequestHandler<WeatherForecastQuery, IEnumerable<WeatherForecast>>
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public Task<IEnumerable<WeatherForecast>> Handle(WeatherForecastQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
        );
    }
}
```

Lastly we just need to swap out our call in `FetchData.razor` with a call to the Mediator:
```c#
forecasts = await Mediator.Send(new WeatherForecastQuery());
```

This allows us to skip the controller all together.

## Authorization
Authorization works the exact same way as it does with controllers & actions.
You can apply the `Authorize` attribute to the request definition (`IClientRequest`) and it will work the same as it does for controllers/actions.

## Setup
1. Add library references
    - In your client/frontend project add a nuget reference to `RemoteMediatr.Client`
    - In your server/backend project add a nuget reference to `RemoteMediatr.Server`
2. Add `builder.Services.AddRemoteMediatr(opt => { opt.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress); });` to your client startup
3. Add `app.MapRemoteMediatrListener(typeof(DemoApp.Shared.Requests).Assembly);` to your server startup
    - `services.AddMediatR` should be called before this
