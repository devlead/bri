using BRI.Commands;
using BRI.Commands.Settings;
using Cake.Core;

namespace BRI.Tests.Unit.Services;

[TestFixture]
public class BicepModuleMarkdownServiceTests
{
    private Module Module { get; set; } = null!;
    private Tag Tag { get; set; } = null!;
    private InventorySettings Settings { get; set; }

    [SetUp]
    public void Init()
    {
        Tag = BriServiceProviderFixture.GetTag();
        Module = BriServiceProviderFixture.GetModule();
        Settings = new InventorySettings
        {
            AcrLoginServer = Constants.ACR.ContainerService,
            OutputPath = "/home/docs",
            TagLimitNumber = 1
        };
    }

    [Test]
    public async Task CreateModuleMarkdown()
    {
        // Given
        var cakecontext = BriServiceProviderFixture.GetRequiredService<ICakeContext>(
                services => services
                                .AddCakeFakes(
                                    fileSystem => fileSystem.CreateDirectory(Settings.OutputPath)
                                )
                                .AddSingleton<InventoryCommand>()
            );

        var bicepModuleMarkdownService = new BicepModuleMarkdownService(Substitute.For<ICakeContext>())
            with
            {
                CakeContext = cakecontext
            };

        // When
        await bicepModuleMarkdownService.CreateModuleMarkdown(
            Settings.AcrLoginServer,
            Settings.OutputPath.Combine(Settings.AcrLoginServer),
            Constants.ACR.Repo,
            Tag,
            Module
            );

        var result = cakecontext.FileSystem.FromDirectoryPath(Settings.OutputPath);

        // Then
        await Verify(result);
    }
}
