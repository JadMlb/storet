using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Storet.Core.Mappers;

public class LowercaseEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
	public override T Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException ($"Unexpected token {reader.TokenType} when parsing enum.");
		
		string maybeLowerCaseEnumValue = reader.GetString()!.Replace ("_", "");
		if (Enum.TryParse<T> (maybeLowerCaseEnumValue, ignoreCase: true, out var value))
			return value;
			
		throw new JsonException ($"Unable to convert '{maybeLowerCaseEnumValue}' to Unit");
	}
	
	public override void Write (Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		string snakeCaseEnumValue = Regex.Replace (
			value.ToString(),
			@"[A-Z]",
			match =>
			{
				var lowerCaseLetter = match.Value.ToLowerInvariant();
				if (match.Index == 0)
					return lowerCaseLetter;
				return "_" + lowerCaseLetter;
			}
		);
		writer.WriteStringValue (snakeCaseEnumValue);
	}
}