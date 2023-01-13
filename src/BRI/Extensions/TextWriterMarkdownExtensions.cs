using BRI.Models;
using System.Globalization;
using System.Text.Json;

namespace BRI.Extensions;

public static  class TextWriterMarkdownExtensions
{
    public const int NameColumnWidth = 35;
    public const int TypeColumnWidth = 55;
    public const int DescriptionColumnWidth = 95;

    public static async Task AddFrontmatter(
        this TextWriter writer,
        AcrRepositoryTag tag,
        string moduleName
        )
    {
        await writer.WriteLineAsync(
            FormattableString.Invariant(
                    $$"""
                    ---
                    summary: Bicep module {{moduleName}}:{{tag.Name}}
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
        AcrRepositoryTag tag,
        string moduleName,
        string moduleFullName
        )
    {
        await writer.WriteLineAsync(
            FormattableString.Invariant(
                    $$"""
                    # Overview
                    
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
        AcrRepositoryModule module
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
            var type = (Value.AllowedValues is string[] values && values.Any())
                ? $"{Value.Type.CodeLine()}<br>{string.Join(
                        ",<br>",
                        values.Select(value => $"'{value}'")
                    ).PreLine()}"
                : Value.Type.CodeLine();

            var required = Value.DefaultValue is JsonElement defaultValue
                ? $"No<br>{defaultValue.ToBicep().Trim().PreLine()}"
                : "Yes";

            await writer.WriteLineAsync(
                $"| {Key.CodeLine(),-NameColumnWidth} | {type,-TypeColumnWidth} | {required.SingleLine(),-90} | {Value.Metadata?.Description.SingleLine(),-DescriptionColumnWidth} |"
                );
        }
    }

    public static async Task AddOutputs(
        this TextWriter writer,
        AcrRepositoryModule module
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
                    $"| {Key.CodeLine(),-NameColumnWidth} | {Value.Type.CodeLine(),-TypeColumnWidth} | {Value.Metadata?.Description.CodeLine(),-DescriptionColumnWidth} |"
                    );
            }
        }
    }

    public static async Task AddBicepExample(
        this TextWriter writer,
        AcrRepositoryModule module,
        string moduleName,
        string moduleFullName
        )
    {
        await writer.WriteLineAsync(
            FormattableString.Invariant(
                    $$"""
                    
                    ## Example
                    
                    ```bicep
                    param env string
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
            if (Value.Type.StartsWith("secure"))
            {
                await writer.WriteLineAsync("@secure()");
            }
            if (Value.AllowedValues is string[] allowedValues && allowedValues.Any())
            {
                await writer.WriteLineAsync($"@allowed([{string.Join(
                        ", ",
                        allowedValues.Select(allowedValue => $"'{allowedValue}'")
                    )}])");
            }
            await writer.WriteLineAsync(
                $"param {Key} {(Value.Type switch { "secureString" => "string", _ => Value.Type })}"
                );
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
                $"    {Key}: {Key}"
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
}
