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
			SolutionConfiguration.SolutionRoot,
			"artifacts/TypeScriptTests"
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
		foreach (var dir in Directory.GetDirectories(DirectoryPath).Where(d => !d.EndsWith("node_modules")))
		{
			// clean out the old folders
			Directory.Delete(dir, recursive: true);
		}

		// Ensure our common package was built
		var tscResult = await NodeUtility.TscOpenApiCodegenTypeScriptPackage(psi => psi.WorkingDirectory = SolutionConfiguration.TypeScriptPackagePath, CancellationToken);
		if (tscResult.ExitCode != 0) throw new InvalidOperationException("tsc failed!") { Data = { ["error"] = tscResult.Error } };

		await WritePackageJson();
		await WriteTsconfigJson();

		var exitCode = await NodeUtility.NpmInstall("typescript@latest", SetupProcess, CancellationToken);
		if (exitCode != 0) throw new InvalidOperationException("npm install failed!");
	}

	private async Task WritePackageJson()
	{
		using var testingPackageJson = typeof(CommonDirectoryFixture).Assembly.GetManifestResourceStream($"{typeof(SolutionConfiguration).Namespace}.package.testing.json");
		if (testingPackageJson == null) throw new InvalidOperationException("Cannot find package.testing.json - make sure the namespace didn't change and it is still embedded.");
		using var streamReader = new StreamReader(testingPackageJson);
		var packageJsonContents = await streamReader.ReadToEndAsync();
		packageJsonContents = packageJsonContents.Replace("%tsfilepath%", SolutionConfiguration.TypeScriptPackagePath.Replace('\\', '/'));
		await File.WriteAllTextAsync(Path.Combine(DirectoryPath, "package.json"), packageJsonContents);
	}

	private async Task WriteTsconfigJson()
	{
		using var testingTsconfigJson = typeof(CommonDirectoryFixture).Assembly.GetManifestResourceStream($"{typeof(SolutionConfiguration).Namespace}.tsconfig.testing.json");
		if (testingTsconfigJson == null) throw new InvalidOperationException("Cannot find tsconfig.testing.json - make sure the namespace didn't change and it is still embedded.");
		using var streamReader = new StreamReader(testingTsconfigJson);
		var tsconfigJsonContents = await streamReader.ReadToEndAsync();
		await File.WriteAllTextAsync(Path.Combine(DirectoryPath, "tsconfig.json"), tsconfigJsonContents);
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
		}
		catch { }
		finally
		{
			cancellation.Dispose();
		}
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
		foreach (var entry in entries)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(generatedFolder, entry.Key))!);

			await File.WriteAllTextAsync(Path.Combine(generatedFolder, entry.Key), contents: entry.SourceText);
		}

		// Ensure the codegenerated files build
		var tscResult = await NodeUtility.Tsc(SetupProcess, CancellationToken);
		if (tscResult.ExitCode != 0)
		{
			var tsconfig = Path.Combine(DirectoryPath, "tsconfig.json");
			var tsconfigContents = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(await File.ReadAllTextAsync(tsconfig));
			var exclude = (System.Text.Json.Nodes.JsonArray)(tsconfigContents!["exclude"] ??= new System.Text.Json.Nodes.JsonArray());
			exclude.Add(documentName + "/");
			await File.WriteAllTextAsync(tsconfig, tsconfigContents.ToJsonString());

			throw new InvalidOperationException($"tsc failed!\n\n{tscResult.Output}");
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
