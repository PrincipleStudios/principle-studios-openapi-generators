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

	private static bool IsWindows =>
		System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

	private static ProcessStartInfo Npx(string args) =>
		IsWindows
			? new ProcessStartInfo("pwsh", @$"-c ""npx {args}""")
			: new ProcessStartInfo("npx", args);

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

}
