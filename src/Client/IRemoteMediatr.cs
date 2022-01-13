using RemoteMediatr.Core;

namespace RemoteMediatr.Client;

public interface IRemoteMediatr
{
    Task Send(IClientRequest request);
    Task<TResponse> Send<TResponse>(IClientRequest<TResponse> request);
}
