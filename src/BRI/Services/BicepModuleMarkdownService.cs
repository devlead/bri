using BRI.Commands.Settings;
using BRI.Extensions;
using BRI.Models;
using Cake.Common.IO;
using Microsoft.Extensions.Logging;

namespace BRI.Services;

public record BicepModuleMarkdownService(
    ILogger<BicepModuleMarkdownService> Logger,
    ICakeContext CakeContext
    )
{
    public async Task CreateModuleMarkdown(InventorySettings settings, DirectoryPath targetPath, string repo, AcrRepositoryTag tag, AcrRepositoryModule module)
    {
        var repoPath = targetPath.Combine(repo);
        CakeContext.EnsureDirectoryExists(repoPath);

        var tagMDPath = repoPath.CombineWithFilePath($"{tag.Name}.md");

        var moduleName = repoPath.GetDirectoryName();
        var moduleFullName = $"br:{settings.AcrLoginServer}/{repo}:{tag.Name}";


        using var stream = CakeContext
                            .FileSystem
                            .GetFile(tagMDPath)
                            .OpenWrite();

        using var writer = new StreamWriter(
            stream,
            System.Text.Encoding.UTF8
            );


        await writer.AddFrontmatter(tag, moduleName);

        await writer.AddOverview(tag, moduleName, moduleFullName);

        await writer.AddParameters(module);

        await writer.AddOutputs(module);

        await writer.AddBicepExample(module, moduleName, moduleFullName);
    }
}
