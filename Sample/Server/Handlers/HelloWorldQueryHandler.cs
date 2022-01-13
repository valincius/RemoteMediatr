using MediatR;
using RemoteMediatr.Core;
using static Sample.Shared.Requests;

namespace Sample.Server.Handlers
{
    public class HelloWorldQueryHandler : IRequestHandler<HelloWorldQuery, string>
    {
        public Task<string> Handle(HelloWorldQuery request, CancellationToken cancellationToken)
        {
            if (request.Name == "John")
                throw new ClientRequestException("You are not allowed to use this API", new List<string> { $"{nameof(request.Name)} cannot be 'John'" });

            if (request.Name == "xxx")
                throw new InvalidOperationException();

            return Task.FromResult($"Hello, {request.Name} - Server time is {DateTime.UtcNow}");
        }
    }
}
