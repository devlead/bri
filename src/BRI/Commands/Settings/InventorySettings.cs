using System.ComponentModel;

namespace BRI.Commands.Settings;

public class InventorySettings : CommandSettings
{
    [CommandArgument(0, "<acrloginserver>")]
    [ValidateString]
    [Description(" Azure Container Registry Login Server")]
    public string AcrLoginServer { get; set; } = string.Empty;

    [CommandArgument(1, "<outputpath>")]
    [ValidatePath]
    [Description(" Output path")]
    public DirectoryPath OutputPath { get; set; } = System.Environment.CurrentDirectory;

    [CommandOption("-t|--tag-limit-number")]
    [Description("Max number of tags to fetch (default 5).")]
    public int TagLimitNumber { get; set; } = 5;

}