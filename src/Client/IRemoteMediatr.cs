using MediatR;

namespace Valincius.RemoteMediatr.Client;

internal interface IRemoteMediatr
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request);
}
