#tool dotnet:?package=GitVersion.Tool&version=5.12.0
#load "build/records.cake"
#load "build/helpers.cake"

/*****************************
 * Setup
 *****************************/
Setup(
    static context => {
         var assertedVersions = context.GitVersion(new GitVersionSettings
            {
                OutputType = GitVersionOutput.Json
            });

        var branchName = assertedVersions.BranchName;
        var isMainBranch = StringComparer.OrdinalIgnoreCase.Equals("main", branchName);

        var gh = context.GitHubActions();
        var buildDate = DateTime.UtcNow;
        var runNumber = gh.IsRunningOnGitHubActions
                            ? gh.Environment.Workflow.RunNumber
                            : (short)((buildDate - buildDate.Date).TotalSeconds/3);

        var version = FormattableString
                    .Invariant($"{buildDate:yyyy.M.d}.{runNumber}");

        context.Information("Building version {0} (Branch: {1}, IsMain: {2})",
            version,
            branchName,
            isMainBranch);

        var artifactsPath = context
                            .MakeAbsolute(context.Directory("./artifacts"));

        var projectRoot =  context
                            .MakeAbsolute(context.Directory("./src"));

        var projectPath = projectRoot.CombineWithFilePath("BRI/BRI.csproj");

        return new BuildData(
            version,
            isMainBranch,
            !context.IsRunningOnWindows(),
            context.BuildSystem().IsLocalBuild,
            projectRoot,
            projectPath,
            new DotNetMSBuildSettings()
                .SetConfiguration("Release")
                .SetVersion(version)
                .WithProperty("Copyright", $"Mattias Karlsson © {DateTime.UtcNow.Year}")
                .WithProperty("Authors", "devlead")
                .WithProperty("Company", "devlead")
                .WithProperty("PackageLicenseExpression", "MIT")
                .WithProperty("PackageTags", "tool;bicep;acr;azure")
                .WithProperty("PackageDescription", "Bicep Registry Inventory .NET Tool - Inventories and documents Bicep modules in a Azure container registry")
                .WithProperty("RepositoryUrl", "https://github.com/devlead/bri.git")
                .WithProperty("ContinuousIntegrationBuild", gh.IsRunningOnGitHubActions ? "true" : "false")
                .WithProperty("EmbedUntrackedSources", "true"),
            artifactsPath,
            artifactsPath.Combine(version)
            );
    }
);

/*****************************
 * Tasks
 *****************************/
Task("Clean")
    .Does<BuildData>(
        static (context, data) => context.CleanDirectories(data.DirectoryPathsToClean)
    )
.Then("Restore")
    .Does<BuildData>(
        static (context, data) => context.DotNetRestore(
            data.ProjectRoot.FullPath,
            new DotNetRestoreSettings {
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("DPI")
    .Does<BuildData>(
        static (context, data) => context.DotNetTool(
                "tool",
                new DotNetToolSettings {
                    ArgumentCustomization = args => args
                                                        .Append("run")
                                                        .Append("dpi")
                                                        .Append("nuget")
                                                        .Append("--silent")
                                                        .AppendSwitchQuoted("--output", "table")
                                                        .Append(
                                                            (
                                                                !string.IsNullOrWhiteSpace(context.EnvironmentVariable("NuGetReportSettings_SharedKey"))
                                                                &&
                                                                !string.IsNullOrWhiteSpace(context.EnvironmentVariable("NuGetReportSettings_WorkspaceId"))
                                                            )
                                                                ? "report"
                                                                : "analyze"
                                                            )
                                                        .AppendSwitchQuoted("--buildversion", data.Version)
                }
            )
    )
.Then("Build")
    .Does<BuildData>(
        static (context, data) => context.DotNetBuild(
            data.ProjectRoot.FullPath,
            new DotNetBuildSettings {
                NoRestore = true,
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("Test")
    .Does<BuildData>(
        static (context, data) => context.DotNetTest(
            data.ProjectRoot.FullPath,
            new DotNetTestSettings {
                NoBuild = true,
                NoRestore = true,
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("Pack")
    .Does<BuildData>(
        static (context, data) => context.DotNetPack(
            data.ProjectPath.FullPath,
            new DotNetPackSettings {
                NoBuild = true,
                NoRestore = true,
                OutputDirectory = data.NuGetOutputPath,
                MSBuildSettings = data.MSBuildSettings
            }
        )
    )
.Then("Upload-Artifacts")
    .WithCriteria(BuildSystem.IsRunningOnGitHubActions, nameof(BuildSystem.IsRunningOnGitHubActions))
    .Does<BuildData>(
        static (context, data) => context
            .GitHubActions()
            .Commands
            .UploadArtifact(data.ArtifactsPath, "artifacts")
    )
.Then("Integration-Tests-Tool-Manifest")
    .Does<BuildData>(
        static (context, data) => context.DotNetTool(
                "new",
                new DotNetToolSettings {
                    ArgumentCustomization = args => args
                                                        .Append("tool-manifest"),
                    WorkingDirectory = data.IntegrationTestPath
                }
            )
    )
.Then("Integration-Tests-Tool-Install")
    .Does<BuildData>(
        static (context, data) =>  context.DotNetTool(
                "tool",
                new DotNetToolSettings {
                    ArgumentCustomization = args => args
                                                        .Append("install")
                                                        .AppendSwitchQuoted("--add-source", data.NuGetOutputPath.FullPath)
                                                        .AppendSwitchQuoted("--version", data.Version)
                                                        .Append("bri"),
                    WorkingDirectory = data.IntegrationTestPath
                }
            )
    )
.Then("Integration-Tests-Tool")
    .WithCriteria<BuildData>((context, data) => data.ShouldRunIntegrationTests(), "ShouldRunIntegrationTests")
    .Does<BuildData>(
        static (context, data) => context.DotNetTool(
                "tool",
                new DotNetToolSettings {
                    ArgumentCustomization = args => args
                                                        .Append("run")
                                                        .Append("--")
                                                        .Append("bri")
                                                        .Append("inventory")
                                                        .AppendQuotedSecret(data.AzureContainerRegistry)
                                                        .AppendQuoted(data.IntegrationTestPath.FullPath),
                    WorkingDirectory = data.IntegrationTestPath
                }
            )
    )
.Then("Integration-Tests-Upload-Results")
    .WithCriteria(BuildSystem.IsRunningOnGitHubActions, nameof(BuildSystem.IsRunningOnGitHubActions))
    .WithCriteria<BuildData>((context, data) => data.ShouldRunIntegrationTests(), "ShouldRunIntegrationTests")
    .Does<BuildData>(
         async (context, data) => {
            var resultPath = data.IntegrationTestPath.Combine(data.AzureContainerRegistry);
            await GitHubActions.Commands.UploadArtifact(
                resultPath,
                data.AzureContainerRegistry
            );
            GitHubActions.Commands.SetStepSummary(
                string.Join(
                    System.Environment.NewLine,
                    context.GetFiles($"{resultPath.FullPath}/**/*.md")
                        .Select(filePath => context.FileSystem.GetFile(filePath).ReadLines(Encoding.UTF8))
                        .SelectMany(line => line)
                )
            );
         }
    )
.Then("Integration-Tests")
.Then("Generate-Statiq-Web")
    .WithCriteria<BuildData>((context, data) => data.ShouldRunIntegrationTests(), "ShouldRunIntegrationTests")
    .Does<BuildData>(static (context, data) => {
        context.DotNetRun(
            data.ProjectRoot.CombineWithFilePath("BRI.TestWeb/BRI.TestWeb.csproj").FullPath,
            new DotNetRunSettings {
                Configuration = "Release",
                NoBuild = true,
                NoRestore = true,
                WorkingDirectory = data.StatiqWebPath,
                ArgumentCustomization = args => args
                                                    .Append("--")
                                                    .AppendSwitchQuoted("--root", data.StatiqWebPath.FullPath)
                                                    .AppendSwitchQuoted("--input", data.IntegrationTestPath.FullPath)
                                                    .AppendSwitchQuoted("--output", data.StatiqWebOutputPath.FullPath)
            }
        );
    })
.Then("Package-Statiq-Web-For-GitHubPages")
    .WithCriteria<BuildData>((context, data) => data.ShouldRunIntegrationTests(), "ShouldRunIntegrationTests")
        .Does<BuildData>(
            static (context, data) => System.Formats.Tar.TarFile.CreateFromDirectoryAsync(
                data.StatiqWebOutputPath.FullPath,
                data.GitHubPagesArtifactPath.FullPath,
                false
             ))
    .Default()
.Then("Upload-GitHubPages-Artifact")
    .WithCriteria<BuildData>((context, data) => data.ShouldRunIntegrationTests(), "ShouldRunIntegrationTests")
    .WithCriteria(BuildSystem.IsRunningOnGitHubActions, nameof(BuildSystem.IsRunningOnGitHubActions))
    .Does<BuildData>(
        static (context, data) => context
            .GitHubActions()
            .Commands
            .UploadArtifact(data.GitHubPagesArtifactPath, "github-pages")
    )
.Then("Push-GitHub-Packages")
    .WithCriteria<BuildData>( (context, data) => data.ShouldPushGitHubPackages())
    .DoesForEach<BuildData, FilePath>(
        static (data, context)
            => context.GetFiles(data.NuGetOutputPath.FullPath + "/*.nupkg"),
        static (data, item, context)
            => context.DotNetNuGetPush(
                item.FullPath,
            new DotNetNuGetPushSettings
            {
                Source = data.GitHubNuGetSource,
                ApiKey = data.GitHubNuGetApiKey
            }
        )
    )
.Then("Push-NuGet-Packages")
    .WithCriteria<BuildData>( (context, data) => data.ShouldPushNuGetPackages())
    .DoesForEach<BuildData, FilePath>(
        static (data, context)
            => context.GetFiles(data.NuGetOutputPath.FullPath + "/*.nupkg"),
        static (data, item, context)
            => context.DotNetNuGetPush(
                item.FullPath,
                new DotNetNuGetPushSettings
                {
                    Source = data.NuGetSource,
                    ApiKey = data.NuGetApiKey
                }
        )
    )
.Then("Create-GitHub-Release")
    .WithCriteria<BuildData>( (context, data) => data.ShouldPushNuGetPackages())
    .Does<BuildData>(
        static (context, data) => context
            .Command(
                new CommandSettings {
                    ToolName = "GitHub CLI",
                    ToolExecutableNames = new []{ "gh.exe", "gh" },
                    EnvironmentVariables = { { "GH_TOKEN", data.GitHubNuGetApiKey } }
                },
                new ProcessArgumentBuilder()
                    .Append("release")
                    .Append("create")
                    .Append(data.Version)
                    .AppendSwitchQuoted("--title", data.Version)
                    .Append("--generate-notes")
                    .Append(string.Join(
                        ' ',
                        context
                            .GetFiles(data.NuGetOutputPath.FullPath + "/*.nupkg")
                            .Select(path => path.FullPath.Quote())
                        ))

            )
    )
.Then("GitHub-Actions")
.Run();
