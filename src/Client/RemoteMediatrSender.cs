using MediatR;
using System.Net.Http.Json;
using System.Text.Json;
using Valincius.RemoteMediatr.Core;

namespace Valincius.RemoteMediatr.Client;

public class RemoteMediatrSender
{
    private readonly HttpClient httpClient;

    public RemoteMediatrSender(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        var requestType = request.GetType();
        var httpRequest = new RemoteMediatrRequest(
            requestType.FullName!,
            typeof(TResponse).FullName!,
            JsonSerializer.Serialize(request, requestType)
        );

        var httpResponse = await httpClient.PostAsJsonAsync(Constants.RequestPath, httpRequest);
        httpResponse.EnsureSuccessStatusCode();
        var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>();
        return response!;
    }
}
