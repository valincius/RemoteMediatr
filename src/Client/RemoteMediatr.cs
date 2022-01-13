using MediatR;
using RemoteMediatr.Core;
using System.Net.Http.Json;
using System.Text.Json;

namespace RemoteMediatr.Client;

public class RemoteMediatr : IRemoteMediatr
{
    private readonly HttpClient _httpClient;

    public RemoteMediatr(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
    }

    private async Task<HttpContent> SendRequest<T>(IClientRequest<T> request)
    {
        var requestType = request.GetType();
        var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
        var content = JsonContent.Create(request, requestType, options: options);
        var httpResponse = await _httpClient.PostAsync($"{Constants.RequestPath}/{requestType.Name}", content);
        if (!httpResponse.IsSuccessStatusCode)
        {
            var problem = await httpResponse.Content.ReadFromJsonAsync<ProblemInfoResponse?>();
            if (problem?.ProblemInfo is not null)
                throw new ClientRequestException(problem.ProblemInfo);

            throw new ApplicationException(httpResponse.ReasonPhrase);
        }

        return httpResponse.Content;
    }

    public async Task Send(IClientRequest request)
    {
        await SendRequest(request);
    }

    public async Task<TResponse> Send<TResponse>(IClientRequest<TResponse> request)
    {
        var response = await SendRequest(request);
        var result = await response.ReadFromJsonAsync<TResponse>();
        return result!;
    }
}
