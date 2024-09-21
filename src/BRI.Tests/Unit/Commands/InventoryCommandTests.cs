using BRI.Commands;
using BRI.Commands.Settings;
using Cake.Core;
using Spectre.Console.Cli;

namespace BRI.Tests.Unit.Commands;

[TestFixture]
public class InventoryCommandTests
{
    [Test]
    public async Task ExecuteAsync()
    {
        // Given
        var context = new CommandContext(
            Array.Empty<string>(),
            Substitute.For<IRemainingArguments>(),
            nameof(InventoryCommand),
            null
            );
        var settings = new InventorySettings
        {
            AcrLoginServer = Constants.ACR.ContainerService,
            OutputPath = "/home/docs",
            TagLimitNumber = 1
        };
        var (
            cakeContext,
            logger,
            catalogService,
            repositoryService,
            bicepModuleMarkdownService
            ) = BriServiceProviderFixture.GetRequiredService<ICakeContext, ILogger<InventoryCommand>, CatalogService, RepositoryService, BicepModuleMarkdownService>(
                services => services
                                .AddSingleton<RepositoryService>()
                                .AddSingleton<BicepModuleMarkdownService>()
                                .AddCakeFakes(
                                    fileSystem => fileSystem.CreateDirectory(settings.OutputPath)
                                )
                                .AddSingleton<InventoryCommand>()
            );

        var command = new InventoryCommand(
            cakeContext,
            logger,
            catalogService,
            repositoryService,
            bicepModuleMarkdownService
            );

        // When
        var result = new
        {
            ExitCode = await command.ExecuteAsync(
                            context,
                            settings
                        ),
            Output = cakeContext.FileSystem.FromDirectoryPath(settings.OutputPath)
        };

        // Then
        await Verify(result);
    }
}
