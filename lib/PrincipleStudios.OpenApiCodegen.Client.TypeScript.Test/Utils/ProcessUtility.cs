using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrincipleStudios.OpenApiCodegen.Client.TypeScript.Utils;

public static class ProcessUtility
{
    public static async Task<int> ExecuteProcess(System.Diagnostics.ProcessStartInfo startInfo, Func<System.Diagnostics.Process, CancellationToken, Task> handleProcess, CancellationToken cancellationToken = default)
    {
        return (await ExecuteProcess(startInfo, async (p, ct) =>
        {
            await handleProcess(p, ct);
            return 0;
        }, cancellationToken)).ExitCode;
    }

    public static async Task<(int ExitCode, T Result)> ExecuteProcess<T>(System.Diagnostics.ProcessStartInfo startInfo, Func<System.Diagnostics.Process, CancellationToken, Task<T>> handleProcess, CancellationToken cancellationToken = default)
    {
        startInfo.RedirectStandardError= true;
        startInfo.RedirectStandardOutput= true;
        startInfo.RedirectStandardInput= true;
        using var process = System.Diagnostics.Process.Start(startInfo);
        if (process == null) throw new InvalidOperationException();

        var registration = cancellationToken.Register(() => process.Kill());
        try
        {
            var result = await handleProcess(process, cancellationToken);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await process.WaitForExitAsync(cts.Token);

            return (process.ExitCode, result);
        }
        finally
        {
            registration.Dispose();
        }
    }
}
