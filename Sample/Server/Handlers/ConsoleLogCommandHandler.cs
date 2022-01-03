using MediatR;
using static Sample.Shared.Requests;

namespace Sample.Server.Handlers
{
    public class ConsoleLogCommandHandler : IRequestHandler<ConsoleLogCommand>
    {
        public Task<Unit> Handle(ConsoleLogCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.Text);
            return Unit.Task;
        }
    }
}
