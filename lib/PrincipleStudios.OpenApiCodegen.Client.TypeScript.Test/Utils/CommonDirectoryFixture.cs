using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;

public class CommonDirectoryFixture : IDisposable
{
	public const string CollectionName = "Common Node directory";
	private readonly CancellationTokenSource cancellation;

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

	private Task Initialize()
	{
		return Task.CompletedTask;
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

	public Task PrepareOpenApiDirectory(string documentName)
	{
		return Task.CompletedTask;
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
