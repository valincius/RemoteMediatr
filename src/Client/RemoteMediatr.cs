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

    public async Task<TResponse> Send<TResponse>(IClientRequest<TResponse> request)
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

        var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>();
        return response!;
    }
}
