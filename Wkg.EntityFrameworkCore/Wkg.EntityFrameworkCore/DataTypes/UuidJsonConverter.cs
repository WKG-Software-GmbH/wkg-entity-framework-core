using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wkg.EntityFrameworkCore.DataTypes;

internal class UuidJsonConverter : JsonConverter<Uuid>
{
    public override Uuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.String)
        {
            throw new JsonException($"Expected a string value for {nameof(Uuid)}.");
        }

        string? value = reader.GetString() ?? throw new JsonException("Unexpected null value for enum.");

        return Uuid.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, Uuid value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
