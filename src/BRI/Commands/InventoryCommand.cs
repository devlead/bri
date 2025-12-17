using Cake.Common.IO;

namespace BRI.Commands;

public class InventoryCommand : AsyncCommand<InventorySettings>
{
    private ICakeContext CakeContext { get; }
    private ILogger Logger { get; }
    private CatalogService CatalogService { get; }
    private RepositoryService RepositoryService { get; }
    private BicepModuleMarkdownService BicepModuleMarkdownService { get; }

    public override async Task<int> ExecuteAsync(CommandContext context, InventorySettings settings, CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        Logger.LogInformation("AcrLoginServer: {AcrLoginServer}", settings.AcrLoginServer);
        Logger.LogInformation("OutputPath: {OutputPath}", settings.OutputPath);

        var targetPath = settings.OutputPath.Combine(settings.AcrLoginServer);

        Logger.LogInformation("Cleaning directory {TargetPath}...", targetPath);
        CakeContext.CleanDirectory(targetPath);
        Logger.LogInformation("Done cleaning directory {TargetPath}.", targetPath);

        Logger.LogInformation("Getting repositories...");
        var repos = await CatalogService.GetRepositories(settings.AcrLoginServer);
        Logger.LogInformation("Found {RepoCount}", repos.Count);
        await Parallel.ForEachAsync(
            repos,
            async (repo, ct) =>
            {
                Logger.LogInformation("Getting tags for {Repo}...", repo);
                var tags = await RepositoryService.Tag.GetTags(
                                settings.AcrLoginServer,
                                repo,
                                settings.TagLimitNumber
                            );
                Logger.LogInformation("Found {TagCount} tags for {Repo}.", tags.Count, repo);

                await Parallel.ForEachAsync(
                    tags,
                    async (tag, ct) =>
                    {
                        Logger.LogInformation("Getting manifest bicep layers for {Repo} tag {Tag}...", repo, tag.Name);
                        var layers = await RepositoryService.Manifest.GetManifestLayers(
                                            settings.AcrLoginServer,
                                            repo,
                                            tag.Digest
                                        );
                        Logger.LogInformation("Found {LayerCount} manifest bicep layers for {Repo} tag {Tag}.", layers.Count, repo, tag.Name);

                        foreach (var layer in   layers)
                        {
                             var module = await RepositoryService.Blob.GetModule(
                                                settings.AcrLoginServer,
                                                repo,
                                                layer.Digest
                                            );

                            await BicepModuleMarkdownService.CreateModuleMarkdown(settings.AcrLoginServer, targetPath, repo, tag, module);
                        }
                    }
                );
            }
        );

        sw.Stop();
        Logger.LogInformation("Processed {RepoCount} in {Elapsed}", repos.Count, sw.Elapsed);

        return 0;
    }

    public InventoryCommand(
        ICakeContext cakeContext,
        ILogger<InventoryCommand> logger,
        CatalogService catalog,
        RepositoryService repository,
        BicepModuleMarkdownService bicepModuleMarkdownService
        )
    {
        CakeContext = cakeContext;
        Logger = logger;
        CatalogService = catalog;
        RepositoryService = repository;
        BicepModuleMarkdownService = bicepModuleMarkdownService;
    }
}
