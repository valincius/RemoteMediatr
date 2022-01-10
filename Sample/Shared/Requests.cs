using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using RemoteMediatr.Core;
using System.Text.Json.Serialization;

namespace Sample.Shared
{
    public class Requests
    {
        public record HelloWorldQuery(string Name) : IClientRequest<string>;

        [Authorize]
        public record WeatherForecastQuery() : IClientRequest<IEnumerable<WeatherForecast>>;

        [Authorize]
        public record ConsoleLogCommand(string Text) : IClientRequest;

        public class UploadFileCommand : IClientRequest<string>
        {
            [JsonIgnore] public Stream File { get; set; }

            public UploadFileCommand(Stream file)
            {
                File = file;
            }
        }
    }
}
