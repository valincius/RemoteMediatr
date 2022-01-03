using RemoteMediatr.Core;
using System.Net.Http.Json;
using System.Text.Json;

namespace RemoteMediatr.Client;

public class RemoteMediatr : IRemoteMediatr
{
    private readonly HttpClient httpClient;

    public RemoteMediatr(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
    }

    public async Task<TResponse> Send<TResponse>(IClientRequest<TResponse> request)
    {
        var requestType = request.GetType();
        var httpRequest = new RemoteMediatrRequest(
            requestType.FullName!,
            JsonSerializer.Serialize(request, requestType)
        );

        var httpResponse = await httpClient.PostAsJsonAsync(Constants.RequestPath, httpRequest);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>();

        return response!;
    }
}
