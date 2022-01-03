using MediatR;
using RemoteMediatr.Core;

namespace RemoteMediatr.Client;

public interface IRemoteMediatr
{
    Task<TResponse> Send<TResponse>(IClientRequest<TResponse> request);
}
