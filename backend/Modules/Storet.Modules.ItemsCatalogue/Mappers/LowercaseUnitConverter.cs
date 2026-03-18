using System.Text.Json;
using System.Text.Json.Serialization;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Mappers;

public class LowercaseUnitConverter : JsonConverter<Unit>
{
	public override Unit Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException ($"Unexpected token {reader.TokenType} when parsing enum.");
		
		string stringValue = reader.GetString()!;
		if (Enum.TryParse<Unit> (stringValue, ignoreCase: true, out var value))
			return value;
			
		throw new JsonException ($"Unable to convert '{stringValue}' to Unit");
	}
	
	public override void Write(Utf8JsonWriter writer, Unit value, JsonSerializerOptions options)
	{
		writer.WriteStringValue (value.ToString().ToLowerInvariant());
	}
}