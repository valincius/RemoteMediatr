using MediatR;
using static Sample.Shared.Requests;

namespace Sample.Server.Handlers
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, string>
    {
        public async Task<string> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            var filePath = Path.Combine(".", Path.GetRandomFileName());
            using var stream = File.Create(filePath);
            await request.File.CopyToAsync(stream, cancellationToken);

            return "Done!";
        }
    }
}
