using System.Text.Json;
using System.Text.Json.Serialization;

namespace Storet.Core.Mappers;

public class LowercaseEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
	public override T Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException ($"Unexpected token {reader.TokenType} when parsing enum.");
		
		string stringValue = reader.GetString()!;
		if (Enum.TryParse<T> (stringValue, ignoreCase: true, out var value))
			return value;
			
		throw new JsonException ($"Unable to convert '{stringValue}' to Unit");
	}
	
	public override void Write (Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		writer.WriteStringValue (value.ToString().ToLowerInvariant());
	}
}