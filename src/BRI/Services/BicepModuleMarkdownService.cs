using Cake.Common.IO;

namespace BRI.Services;

public record BicepModuleMarkdownService(ICakeContext CakeContext)
{
    public async Task CreateModuleMarkdown(string containerService, DirectoryPath targetPath, string repo, Tag tag, Module module)
    {
        var repoPath = targetPath.Combine(repo);
        CakeContext.EnsureDirectoryExists(repoPath);

        var tagMDPath = repoPath.CombineWithFilePath($"{tag.Name}.md");

        var moduleName = repoPath.GetDirectoryName();
        var moduleFullName = $"br:{containerService}/{repo}:{tag.Name}";


        using var stream = CakeContext
                            .FileSystem
                            .GetFile(tagMDPath)
                            .OpenWrite();

        using var writer = new StreamWriter(
            stream,
            Encoding.UTF8
            );

        await writer.AddFrontmatter(tag, moduleName, module.Metadata?.Documentation?.Summary);

        await writer.AddOverview(tag, moduleName, moduleFullName, module.Metadata?.Documentation?.Description);

        await writer.AddParameters(module);

        await writer.AddOutputs(module);

        await writer.AddBicepExample(module, moduleName, moduleFullName);
    }
}
