using PrincipleStudios.OpenApi.Transformations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;

using static PrincipleStudios.OpenApiCodegen.TestUtils.DocumentHelpers;
using static PrincipleStudios.OpenApiCodegen.Client.TypeScript.OptionsHelpers;

public class CommonDirectoryFixture : IDisposable
{
    public const string CollectionName = "Common Node directory";
    private readonly CancellationTokenSource cancellation;
    private readonly ConcurrentDictionary<string, Task> openapiDirectories = new();

    public CommonDirectoryFixture()
    {
        DirectoryPath = Path.Combine(
            Path.GetTempPath(),
            "PS-openapicodegen-tests",
            DateTime.Now.ToString("yyyy-MM-dd-hhmmss-") + Path.GetRandomFileName()
        );
        Directory.CreateDirectory(DirectoryPath);

        cancellation = new CancellationTokenSource();

        Initialized = Initialize();
    }
    public string DirectoryPath { get; private set; }


    public Task Initialized { get; }
    public CancellationToken CancellationToken => cancellation.Token;

    private async Task Initialize()
    {
        // Ensure our common package was built
        var tscResult = await NodeUtility.Tsc(psi => psi.WorkingDirectory = SolutionConfiguration.TypeScriptPackagePath, CancellationToken);
        if (tscResult.ExitCode != 0) throw new InvalidOperationException("tsc failed!") { Data = { ["error"] = tscResult.Error } };

        using var testingPackageJson = typeof(CommonDirectoryFixture).Assembly.GetManifestResourceStream($"{typeof(SolutionConfiguration).Namespace}.package.testing.json");
        if (testingPackageJson == null) throw new InvalidOperationException("Cannot find package.testing.json - make sure the namespace didn't change and it is still embedded.");
        using var streamReader = new StreamReader(testingPackageJson);
        var packageJsonContents = await streamReader.ReadToEndAsync();
        packageJsonContents = packageJsonContents.Replace("%tsfilepath%", SolutionConfiguration.TypeScriptPackagePath.Replace('\\', '/'));
        await File.WriteAllTextAsync(Path.Combine(DirectoryPath, "package.json"), packageJsonContents);

        var exitCode = await NodeUtility.NpmInstall(SetupProcess, CancellationToken);
        if (exitCode != 0) throw new InvalidOperationException("npm install failed!");
    }

    public void SetupProcess(System.Diagnostics.ProcessStartInfo psi)
    {
        psi.WorkingDirectory = DirectoryPath;
    }

    public void Dispose()
    {
        try
        {
            cancellation.Cancel();
            Initialized.Wait();

            System.IO.Directory.Delete(DirectoryPath, true);
        }
        catch { }
    }

    private async Task GenerateTypeScriptFrom(string documentName)
    {
        await Initialized;

        var document = GetDocument(documentName);
        var options = LoadOptions();

        var transformer = document.BuildTypeScriptOperationSourceProvider("", options);
        OpenApiTransformDiagnostic diagnostic = new();

        var entries = transformer.GetSources(diagnostic).ToArray();
        var generatedFolder = Path.Combine(DirectoryPath, documentName);
        Directory.CreateDirectory(generatedFolder);
        foreach ( var entry in entries)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(generatedFolder, entry.Key))!);

            File.WriteAllText(Path.Combine(generatedFolder, entry.Key), contents: entry.SourceText);
        }
    }

    public Task PrepareOpenApiDirectory(string documentName)
    {
        lock (openapiDirectories)
        {
            return openapiDirectories.GetOrAdd(documentName, GenerateTypeScriptFrom);
        }
    }

    internal async Task<NodeUtility.ProcessResult> TsNode(string source)
    {
        await Initialized;
        return await NodeUtility.TsNode(source, SetupProcess);
    }
}

[CollectionDefinition(CommonDirectoryFixture.CollectionName)]
public class CommonDirectoryCollection : ICollectionFixture<CommonDirectoryFixture>
{
}
