using MediatR;

namespace Valincius.RemoteMediatr.Core;

public interface IClientRequest<TResponse> : IRequest<TResponse>
{
}

public interface IClientRequest : IClientRequest<Unit>
{
}
