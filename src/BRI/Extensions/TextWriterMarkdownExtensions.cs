namespace BRI.Extensions;

public static  class TextWriterMarkdownExtensions
{
    public const int NameColumnWidth = 35;
    public const int TypeColumnWidth = 55;
    public const int DescriptionColumnWidth = 95;

    public static async Task AddFrontmatter(
        this TextWriter writer,
        Tag tag,
        string moduleName,
        string? summary)
    {
        await writer.WriteLineAsync(
            FormattableString.Invariant(
                    $$"""
                    ---
                    summary: {{(
                        string.IsNullOrWhiteSpace(summary)
                            ? $"Bicep module {moduleName}:{tag.Name}"
                            : summary
                            )}}
                    modifiedby: BRI
                    modified: {{tag.LastUpdateTime:yyyy-MM-dd HH:mm}}
                    order: -{{tag.LastUpdateTime:yyyyMMddHHmm}}
                    ---
                    """
                )
            );
    }

    public static async Task AddOverview(
        this TextWriter writer,
        Tag tag,
        string moduleName,
        string moduleFullName,
        string? description)
    {
        await writer.WriteLineAsync(
            FormattableString.Invariant(
                    $$"""
                    # Overview
                    {{(
                    string.IsNullOrWhiteSpace(description)
                        ? string.Empty
                        : $"{Environment.NewLine}{description.Trim().NormalizeLineEndings()}{Environment.NewLine}"
                    )}}
                    |                                     |                                                                                                 |
                    |-------------------------------------|-------------------------------------------------------------------------------------------------|
                    | **Name**                            | {{moduleName.CodeLine(),-DescriptionColumnWidth}} |
                    | **Tag**                             | {{tag.Name.CodeLine(),-DescriptionColumnWidth}} |
                    | **FullName**                        | {{moduleFullName.CodeLine(),-DescriptionColumnWidth}} |
                    | **Last updated**                    | {{tag.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss:zzz", CultureInfo.InvariantCulture).CodeLine(),-DescriptionColumnWidth}} |
                    | **Created**                         | {{tag.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss:zzz", CultureInfo.InvariantCulture).CodeLine(),-DescriptionColumnWidth}} |
                    | **Digest**                          | {{tag.Digest.CodeLine(),-DescriptionColumnWidth}} |
                    
                    """
                )
            );
    }

    public static async Task AddParameters(
        this TextWriter writer,
        Module module
        )
    {
        await writer.WriteLineAsync(
            FormattableString.Invariant(
                $$"""
                ## Parameters
                
                | Name                                | Type                                                    | Required                                                                                   | Description                                                                                     |
                |-------------------------------------|---------------------------------------------------------|--------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------|
                """
            )
        );

        foreach (var (Key, Value) in module.Parameters)
        {
            var type = Value.GetParameterType();

            var required = Value.DefaultValue is JsonElement defaultValue
                ? $"No<br>{defaultValue.ToBicep().Trim().PreLine()}"
                : "Yes";

            await writer.WriteLineAsync(
                $"| {Key.CodeLine(),-NameColumnWidth} | {type,-TypeColumnWidth} | {required.SingleLine(),-90} | {Value.Metadata?.Description.SingleLine(),-DescriptionColumnWidth} |"
                );
        }
    }

    private static string GetParameterType(this Parameter value)
    {
        var typeBuilder = new StringBuilder();
        if (value.ParsedType.Secure)
        {
            typeBuilder.Append("**▲ secure ▲**<br>");
        }
        typeBuilder.Append(value.ParsedType.Type.CodeLine());

        if(value.AllowedValues is JsonElement[] values && values.Any())
        {
            typeBuilder.Append("<br>");
            typeBuilder.Append(string.Join(
                        ",<br>",
                        values.Select(value => value.ToBicep())
                    ).PreLine());
        }

        typeBuilder.AppendRangeIfSet("Value", value.MinValue, value.MaxValue);
        typeBuilder.AppendRangeIfSet("Length", value.MinLength, value.MaxLength);

        return typeBuilder.ToString();
    }

    private static void AppendRangeIfSet(
        this StringBuilder stringBuilder,
        string key,
        object? min,
        object? max
        )
    {
        if (min == null && max == null)
        {
            return;
        }

        stringBuilder.Append("<br>");
        stringBuilder.Append(key);
        stringBuilder.Append(':');
        if (min != null)
        {
            stringBuilder.Append(' ');
            stringBuilder.Append(FormattableString.Invariant($"≧{min}").CodeLine());
        }
        if (max != null)
        {
            stringBuilder.Append(' ');
            stringBuilder.Append(FormattableString.Invariant($"≦{max}").CodeLine());
        }
    }

    public static async Task AddOutputs(
        this TextWriter writer,
        Module module
        )
    {
        await writer.WriteLineAsync(
            FormattableString.Invariant(
                $$"""
                
                ## Outputs
                
                | Name                                | Type                                                    | Description                                                                                     |
                |-------------------------------------|---------------------------------------------------------|-------------------------------------------------------------------------------------------------|
                """
            )
        );

        if (module.Outputs != null)
        {
            foreach (var (Key, Value) in module.Outputs)
            {
                await writer.WriteLineAsync(
                    $"| {Key.CodeLine(),-NameColumnWidth} | {Value.Type.CodeLine(),-TypeColumnWidth} | {Value.Metadata?.Description.SingleLine(),-DescriptionColumnWidth} |"
                    );
            }
        }
    }

    public static async Task AddBicepExample(
        this TextWriter writer,
        Module module,
        string moduleName,
        string moduleFullName
        )
    {
        await writer.WriteLineAsync(
            FormattableString.Invariant(
                    $$"""
                    
                    ## Example
                    
                    ```bicep
                    @description('The target environment.')
                    param env string

                    @description('The deployment version.')
                    param version string = '1.0.0.0'

                    """
                )
            );

        foreach (var (Key, Value) in module.Parameters)
        {
            switch (Key)
            {
                case "env":
                case "version":
                    continue;
            }

            if (Value.DefaultValue is not null)
            {
                continue;
            }

            await writer.WriteFunctionLineIfValueSet("description", Value.Metadata?.Description);

            if (Value.ParsedType.Secure)
            {
                await writer.WriteLineAsync("@secure()");
            }

            await writer.WriteFunctionLineIfValueSet("allowed", Value.AllowedValues);

            await writer.WriteFunctionLineIfValueSet("minValue", Value.MinValue);

            await writer.WriteFunctionLineIfValueSet("maxValue", Value.MaxValue);

            await writer.WriteFunctionLineIfValueSet("minLength", Value.MinLength);

            await writer.WriteFunctionLineIfValueSet("maxLength", Value.MaxLength);

            await writer.WriteLineAsync(
                $"param {Key} {Value.ParsedType.Type}"
                );

            await writer.WriteLineAsync();
        }

        await writer.WriteLineAsync(
           FormattableString.Invariant(
                    $$"""
                    
                    module {{moduleName}} '{{moduleFullName}}' = {
                        name: '{{moduleName}}${env}${version}'
                        params: {
                    """
                )
           );

        foreach (var (Key, Value) in module.Parameters)
        {
            if (Value.DefaultValue is not null)
            {
                continue;
            }

            await writer.WriteLineAsync(
                $"      {Key}: {Key}"
                );
        }

        await writer.WriteLineAsync(
            FormattableString.Invariant(
                   $$"""
                        }
                    }
                    ```
                    """
                )
            );
    }

    private static async Task WriteFunctionLineIfValueSet(
       this TextWriter writer,
       string functionName,
       object? value
       )
    {
        string functionValue;
        switch (value)
        {
            case int intValue:
                functionValue = intValue.ToString(CultureInfo.InvariantCulture);
                break;
            case string stringValue:
                functionValue = $"'{stringValue}'";
                break;
            case bool boolValue:
                functionValue = boolValue ? "true" : "false";
                break;

            case string[] { Length:>0 } stringValues:
                functionValue =
                        $$"""
                        [
                            {{string.Join(
                                $"{Environment.NewLine}  ",
                                stringValues.Select(stringValue => $"'{stringValue}'")
                                )}}
                        ]
                        """;
                break;
            default:
                return;
        }

        await writer.WriteLineAsync($"@{functionName}({functionValue})");
    }
}
