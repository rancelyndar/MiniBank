using System.Text.Json;
using System.Text.Json.Serialization;
using MiniBank.Core;

public class JsonCurrencyConverter : JsonConverter<Currency>
{
    private readonly JsonSerializerOptions ConverterOptions;
    public JsonCurrencyConverter(JsonSerializerOptions converterOptions)
    {
        ConverterOptions = converterOptions;
    }
    public override Currency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<Currency>(ref reader, ConverterOptions);
    }

    public override void Write(Utf8JsonWriter writer, Currency value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize<Currency>(writer, value, ConverterOptions);
    }
}