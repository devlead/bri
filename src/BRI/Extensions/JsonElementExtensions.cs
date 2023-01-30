using System.Text.RegularExpressions;

namespace BRI.Extensions;

public static partial class JsonElementExtensions
{
    [GeneratedRegex("parameters\\('(?<value>[\\s\\S]*?)'\\)", RegexOptions.NonBacktracking | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant)]
    private static partial Regex ParametersRegex();

    public static string ToBicep(this JsonElement element, int level = 0)
        => element switch
        {
            { ValueKind: JsonValueKind.String } => StringToBicep(element),
            { ValueKind: JsonValueKind.True } => "true",
            { ValueKind: JsonValueKind.False } => "false",
            { ValueKind: JsonValueKind.Array } => ArrayToBicep(element, level),
            { ValueKind: JsonValueKind.Object } => ObjectToBicep(element, level),
            { ValueKind: JsonValueKind.Number } => element.GetRawText(),
            _ => element.GetRawText()
        };

    private static string ArrayToBicep(JsonElement element, int level)
    {
        var indent = GetLevelIndent(level);
        return $"{indent}[{string.Concat(
                            element.EnumerateArray().Select(value => $"\r\n{indent}  {value.ToBicep()}")
                )}\r\n{indent}]";
    }

    private static string StringToBicep(JsonElement element)
    {
        var elementValue = element.GetString();
        return elementValue switch
        {
            string { Length: > 2 } value when value[0] == '[' && value[^1] == ']' => ParametersRegex().Replace(
            value[1..^1],
            m => m.Groups["value"].Value
            ),
            _ => $"'{elementValue?.Trim('"')}'"
        };
    }

    private static string ObjectToBicep(JsonElement element, int level)
    {
        var indent = GetLevelIndent(level);
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

    private static string GetLevelIndent(int level)
        => level switch
        {
            0 => string.Empty,
            1 => "  ",
            2 => "    ",
            3 => "      ",
            4 => "        ",
            5 => "          ",
            6 => "            ",
            7 => "              ",
            8 => "                ",
            9 => "                  ",
            _ => "".PadLeft(level * 2)
        };
}
