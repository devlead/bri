using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace BRI.Extensions;

public static class JsonElementExtensions
{
    public static string ToBicep(this JsonElement element, int level = 0)
        => element switch
        {
            { ValueKind: JsonValueKind.String } => StringToBicep(element),
            { ValueKind: JsonValueKind.True } => "true",
            { ValueKind: JsonValueKind.False } => "false",
            { ValueKind: JsonValueKind.Array } => $"[{string.Join(
                    ", ",
                    element.EnumerateArray().Select(value => value.ToBicep())
                )}]",
            { ValueKind: JsonValueKind.Object } => ObjectToBicep(element, level),
            { ValueKind: JsonValueKind.Number } => element.GetRawText(),
            _ => element.GetRawText()
        };

    private static string StringToBicep(JsonElement element)
    {
        var elementValue = element.GetString();
        return elementValue switch {
            string { Length: > 2 } value when value[0]=='[' && value[^1]==']'  => value[1..^1],
             _ => $"'{elementValue?.Trim('"')}'"
            };
    }

    private static string ObjectToBicep(JsonElement element, int level)
    {
        string indent = "".PadLeft(level * 2);
        return element
                .EnumerateObject()
                .Aggregate(
                    new StringBuilder()
                        .Append(indent)
                        .AppendLine("{"),
                    (builder, property) => builder
                                            .Append(indent)
                                            .AppendLine($"  {property.Name}: {property.Value.ToBicep(level + 1)}"),
                    builder => builder
                            .Append(indent)
                                .AppendLine("}")
                                .ToString()
                    );
    }
}
