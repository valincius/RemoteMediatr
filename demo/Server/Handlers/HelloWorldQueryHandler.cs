using MediatR;
using static DemoApp.Shared.Requests;

namespace DemoApp.Server.Handlers
{
    public class HelloWorldQueryHandler : IRequestHandler<HelloWorldQuery, string>
    {
        public Task<string> Handle(HelloWorldQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Hello, {request.Name} - Server time is {DateTime.UtcNow}");
        }
    }
}
