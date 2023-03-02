using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;

using static ProcessUtility;

static class NodeUtility
{
#if Windows
    public const string npmCli = "npm.cmd";
    public const string npxCli = "npx.cmd";
#else
    public const string npmCli = "npm";
    public const string npxCli = "npx";
#endif

    public record ProcessResult(int ExitCode, string Output, string Error);

    public static async Task<ProcessResult> TsNode(string input, Action<System.Diagnostics.ProcessStartInfo>? configureStartInfo = null, CancellationToken cancellationToken = default)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo(npxCli, "-y ts-node");
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

    public static async Task<int> NpmInstall(Action<System.Diagnostics.ProcessStartInfo>? configureStartInfo = null, CancellationToken cancellationToken = default)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo(npmCli, "install");
        configureStartInfo?.Invoke(startInfo);
        var (exitCode, (output, error)) = await ExecuteProcess(startInfo, async (process, cancellationToken) =>
        {
            process.StandardInput.Close();
            var result = await Task.WhenAll(process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync());
            return (Output: result[0], Error: result[1]);
        }, cancellationToken);

        return exitCode;
    }

    public static async Task<ProcessResult> Tsc(Action<System.Diagnostics.ProcessStartInfo>? configureStartInfo = null, CancellationToken cancellationToken = default)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo(npxCli, "tsc --project tsconfig.build.json");
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
