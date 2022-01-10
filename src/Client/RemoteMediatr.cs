﻿using RemoteMediatr.Core;
using System.Net.Http.Json;
using System.Reflection;
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

        using var content = new MultipartFormDataContent
        {
            { new StringContent(JsonSerializer.Serialize(request, requestType)), "Body" }
        };

        var streamProperties = requestType.GetProperties()
            .Where(x => x.PropertyType.IsAssignableFrom(typeof(Stream)));
        foreach (var prop in streamProperties)
            content.Add(new StreamContent((prop.GetValue(request) as Stream)!), prop.Name, prop.Name);

        var httpResponse = await httpClient.PostAsync($"{Constants.RequestPath}/{requestType.Name}", content);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>();
        return response!;
    }
}
