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
        var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
        var content = JsonContent.Create(request, requestType, options: options);
        var httpResponse = await httpClient.PostAsync($"{Constants.RequestPath}/{requestType.Name}", content);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>();
        return response!;
    }
}
