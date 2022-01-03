using MediatR;
using static Sample.Shared.Requests;

namespace Sample.Server.Handlers
{
    public class HelloWorldQueryHandler : IRequestHandler<HelloWorldQuery, string>
    {
        public Task<string> Handle(HelloWorldQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Hello, {request.Name} - Server time is {DateTime.UtcNow}");
        }
    }
}
