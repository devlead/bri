namespace BRI.Tests.Unit.Extensions;

[TestFixture]
public class TextWriterMarkdownExtensionsTests
{
    private Module Module { get; set; } = null!;
    private Tag Tag { get; set; } = null!;
    private string ModuleName { get; set; } = null!;
    private string ModuleFullName { get; set; } = null!;

    [SetUp]
    public void Init()
    {
        Tag = BriServiceProviderFixture.GetTag();
        Module = BriServiceProviderFixture.GetModule();
        ModuleName = Path.GetFileName(Constants.ACR.Repo);
        ModuleFullName = $"br:{Constants.ACR.ContainerService}/{Constants.ACR.Repo}:{Tag.Name}";
    }

    [Test]
    public async Task AddFrontmatter()
    {
        // Given
        var sw = new StringWriter();

        // When
        await sw.AddFrontmatter(Tag, ModuleName, Module.Metadata.Documentation?.Summary);

        // Then
        await Verify(sw);
    }

    [Test]
    public async Task AddOverview()
    {
        // Given
        var sw = new StringWriter();

        // When
        await sw.AddOverview(Tag, ModuleName, ModuleFullName, Module.Metadata.Documentation?.Description);

        // Then
        await Verify(sw);
    }

    [Test]
    public async Task AddParameters()
    {
        // Given
        var sw = new StringWriter();

        // When
        await sw.AddParameters(Module);

        // Then
        await Verify(sw);
    }

    [Test]
    public async Task AddOutputs()
    {
        // Given
        var sw = new StringWriter();

        // When
        await sw.AddOutputs(Module);

        // Then
        await Verify(sw);
    }

    [Test]
    public async Task AddBicepExample()
    {
        // Given
        var sw = new StringWriter();

        // When
        await sw.AddBicepExample(Module, ModuleName, ModuleFullName);

        // Then
        await Verify(sw);
    }
}