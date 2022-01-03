using MediatR;
using Valincius.RemoteMediatr.Core;

namespace Valincius.RemoteMediatr.Client;

public interface IRemoteMediatr
{
    Task<TResponse> Send<TResponse>(IClientRequest<TResponse> request);
}
