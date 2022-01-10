using System.Text.Json;
using System.Text.Json.Serialization;

namespace RemoteMediatr.Client;
internal class StreamJsonConverter : JsonConverter<Stream>
{
    public override Stream? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return null;
    }

    public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
    {
        writer.WriteNullValue();
    }
}
