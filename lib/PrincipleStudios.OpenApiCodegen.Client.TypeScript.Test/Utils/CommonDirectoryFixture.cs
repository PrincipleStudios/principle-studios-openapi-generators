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
			"lib/PrincipleStudios.OpenApiCodegen.Client.TypeScript.Test/testing"
		);

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

		await Task.Yield();
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
