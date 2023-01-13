using BRI.Commands.Settings;
using BRI.Services;
using Cake.Common.IO;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.Diagnostics;

namespace BRI.Commands;

public class InventoryCommand : AsyncCommand<InventorySettings>
{
    private ICakeContext CakeContext { get; }
    private ILogger Logger { get; }
    private AcrCatalogService AcrCatalogService { get; }
    private AcrRepositoryTagService AcrRepositoryTagService { get; }
    private AcrRepositoryManifestService AcrRepositoryManifestService { get; }
    private AcrRepositoryBlobService AcrRepositoryBlobService { get; }
    private BicepModuleMarkdownService BicepModuleMarkdownService { get; }

    public override async Task<int> ExecuteAsync(CommandContext context, InventorySettings settings)
    {
        var sw = Stopwatch.StartNew();
        Logger.LogInformation("AcrLoginServer: {AcrLoginServer}", settings.AcrLoginServer);
        Logger.LogInformation("OutputPath: {OutputPath}", settings.OutputPath);

        var targetPath = settings.OutputPath.Combine(settings.AcrLoginServer);

        Logger.LogInformation("Cleaning directory {TargetPath}...", targetPath);
        CakeContext.CleanDirectory(targetPath);
        Logger.LogInformation("Done cleaning directory {TargetPath}.", targetPath);

        Logger.LogInformation("Getting repositories...");
        var repos = await AcrCatalogService.GetRepositories(settings.AcrLoginServer);
        Logger.LogInformation("Found {RepoCount}", repos.Count);

        foreach(var repo in repos)
        {
            Logger.LogInformation("Getting tags for {Repo}...", repo);
            var tags = await AcrRepositoryTagService.GetTags(
                            settings.AcrLoginServer,
                            repo,
                            settings.TagLimitNumber
                        );
            Logger.LogInformation("Found {TagCount} tags for {Repo}.", tags.Count, repo);

            foreach(var tag in tags)
            {
                Logger.LogInformation("Getting manifest bicep layers for {Repo} tag {Tag}...", repo, tag.Name);
                var layers = await AcrRepositoryManifestService.GetManifestLayers(
                                    settings.AcrLoginServer,
                                    repo,
                                    tag.Digest
                                );
                Logger.LogInformation("Found {LayerCount} manifest bicep layers for {Repo} tag {Tag}.", layers.Count, repo, tag.Name);

                foreach(var layer in layers)
                {
                    var module = await AcrRepositoryBlobService.GetModule(
                                        settings.AcrLoginServer,
                                        repo,
                                        layer.Digest
                                    );

                    await BicepModuleMarkdownService.CreateModuleMarkdown(settings, targetPath, repo, tag, module);
                }
            }
        }

        sw.Stop();
        Logger.LogInformation("Processed {RepoCount} in {Elapsed}", repos.Count, sw.Elapsed);

        return 0;
    }

    public InventoryCommand(
        ICakeContext cakeContext,
        ILogger<InventoryCommand> logger,
        AcrCatalogService acrCatalogService,
        AcrRepositoryTagService acrRepositoryTagService,
        AcrRepositoryManifestService acrRepositoryManifestService,
        AcrRepositoryBlobService acrRepositoryBlobService,
        BicepModuleMarkdownService bicepModuleMarkdownService
        )
    {
        CakeContext = cakeContext;
        Logger = logger;
        AcrCatalogService = acrCatalogService;
        AcrRepositoryTagService = acrRepositoryTagService;
        AcrRepositoryManifestService = acrRepositoryManifestService;
        AcrRepositoryBlobService = acrRepositoryBlobService;
        BicepModuleMarkdownService = bicepModuleMarkdownService;
    }
}
