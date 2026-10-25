using System.Diagnostics;

namespace ParcelServices;

public sealed record ShellResult(int ExitCode, string Output, string Error, bool TimedOut);

public sealed class ShellService
{
    public async Task<ShellResult> ExecuteAsync(
        string command,
        string workingDirectory,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command))
            throw new ArgumentException("A shell command is required.", nameof(command));
        if (!Directory.Exists(workingDirectory))
            throw new DirectoryNotFoundException($"Working directory not found: {workingDirectory}");

        var startInfo = OperatingSystem.IsWindows()
            ? new ProcessStartInfo
            {
                FileName = "cmd.exe",
                ArgumentList = { "/d", "/s", "/c", command }
            }
            : new ProcessStartInfo
            {
                FileName = "/bin/sh",
                ArgumentList = { "-c", command }
            };

        startInfo.WorkingDirectory = workingDirectory;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start the shell process.");

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var waitTask = process.WaitForExitAsync(cancellationToken);
        var completedTask = await Task.WhenAny(waitTask, Task.Delay(timeout, cancellationToken));

        if (completedTask != waitTask)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
                // The process exited between the timeout and the kill request.
            }

            await process.WaitForExitAsync(CancellationToken.None);
            return new ShellResult(-1, await outputTask, await errorTask, true);
        }

        await waitTask;
        return new ShellResult(process.ExitCode, await outputTask, await errorTask, false);
    }
}
