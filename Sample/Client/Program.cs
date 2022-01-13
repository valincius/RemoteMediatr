using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RemoteMediatr.Client;
using Sample.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRemoteMediatrClient(httpClient => {
    httpClient.ConfigureHttpClient(opt => opt.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
    httpClient.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
});

builder.Services.AddApiAuthorization();

builder.Logging.SetMinimumLevel(LogLevel.Warning);

await builder.Build().RunAsync();
