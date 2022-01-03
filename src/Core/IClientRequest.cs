using MediatR;

namespace RemoteMediatr.Core;

public interface IClientRequest<TResponse> : IRequest<TResponse>
{
}

public interface IClientRequest : IClientRequest<Unit>
{
}
