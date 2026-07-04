using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace CitySim.Helpers.Converters;

public class Vector2IJsonConverter : JsonConverter<Vector2I>
{
    public override Vector2I Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int x = 0, y = 0;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            if (string.Equals(propertyName, "x", StringComparison.OrdinalIgnoreCase))
                x = reader.GetInt32();
            else if (string.Equals(propertyName, "y", StringComparison.OrdinalIgnoreCase))
                y = reader.GetInt32();
        }

        return new Vector2I(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Vector2I value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteEndObject();
    }
}