using MediatR;
using static Sample.Shared.Requests;

namespace Sample.Server.Handlers
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, string>
    {
        public async Task<string> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            using var streamReader = new StreamReader(request.File);
            var content = await streamReader.ReadToEndAsync();
            return content;
        }
    }
}
