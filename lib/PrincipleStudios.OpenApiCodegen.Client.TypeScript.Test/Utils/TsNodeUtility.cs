using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;

using static ProcessUtility;

static class NodeUtility
{
	public record ProcessResult(int ExitCode, string Output, string Error);

	private static ProcessStartInfo Npx(string args) =>
#if Windows
		new ProcessStartInfo("pwsh", @$"-c ""npx {args}""");
#else
        new ProcessStartInfo("npx", args);
#endif
	private static ProcessStartInfo Npm(string args) =>
#if Windows
		new ProcessStartInfo("pwsh", @$"-c ""npm {args}""");
#else
        new ProcessStartInfo("npm", args);
#endif

	public static async Task<ProcessResult> TsNode(string input, Action<System.Diagnostics.ProcessStartInfo>? configureStartInfo = null, CancellationToken cancellationToken = default)
	{
		var startInfo = Npx("-y ts-node");
		configureStartInfo?.Invoke(startInfo);
		var (exitCode, (output, error)) = await ExecuteProcess(startInfo, async (process, cancellationToken) =>
		{
			var sw = process.StandardInput;
			await sw.WriteAsync(input);
			await sw.FlushAsync();
			sw.Close();

			var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync());
			return (Output: result[0], Error: result[1]);
		}, cancellationToken);

		return new ProcessResult(ExitCode: exitCode, Output: output, Error: error);
	}

	public static async Task<int> NpmInstall(string additionalPackages, Action<System.Diagnostics.ProcessStartInfo>? configureStartInfo = null, CancellationToken cancellationToken = default)
	{
		var startInfo = Npm($"install {additionalPackages}");
		configureStartInfo?.Invoke(startInfo);
		var (exitCode, (output, error)) = await ExecuteProcess(startInfo, async (process, cancellationToken) =>
		{
			process.StandardInput.Close();
			var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync());
			return (Output: result[0], Error: result[1]);
		}, cancellationToken);

		return exitCode;
	}

	public static async Task<ProcessResult> TscOpenApiCodegenTypeScriptPackage(Action<System.Diagnostics.ProcessStartInfo>? configureStartInfo = null, CancellationToken cancellationToken = default)
	{
		var startInfo = Npx("tsc --project tsconfig.build.json");
		configureStartInfo?.Invoke(startInfo);
		var (exitCode, (output, error)) = await ExecuteProcess(startInfo, async (process, cancellationToken) =>
		{
			var sw = process.StandardInput;
			sw.Close();

			var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync());
			return (Output: result[0], Error: result[1]);
		}, cancellationToken);

		return new ProcessResult(ExitCode: exitCode, Output: output, Error: error);
	}

	public static async Task<ProcessResult> Tsc(Action<System.Diagnostics.ProcessStartInfo>? configureStartInfo = null, CancellationToken cancellationToken = default)
	{
		var startInfo = Npx("tsc --project tsconfig.json");
		configureStartInfo?.Invoke(startInfo);
		var (exitCode, (output, error)) = await ExecuteProcess(startInfo, async (process, cancellationToken) =>
		{
			var sw = process.StandardInput;
			sw.Close();

			var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync());
			return (Output: result[0], Error: result[1]);
		}, cancellationToken);

		return new ProcessResult(ExitCode: exitCode, Output: output, Error: error);
	}

}
