using System.Diagnostics;

namespace ParcelServices
{
    public class GitService
    {
        private string _repoPath = string.Empty;

        public GitService(string repoPath)
        {
            SetRepository(repoPath);
        }

        public void SetRepository(string repoPath)
        {
            if (string.IsNullOrWhiteSpace(repoPath) || !Directory.Exists(repoPath))
                throw new DirectoryNotFoundException($"Git working directory not found: {repoPath}");
            _repoPath = Path.GetFullPath(repoPath);
        }

        public string RunGitCommand(string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = _repoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return "Error starting git process.";

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            var result = string.IsNullOrWhiteSpace(output) ? string.Empty : output.TrimEnd();
            if (!string.IsNullOrWhiteSpace(error))
                result += $"{Environment.NewLine}Error: {error.TrimEnd()}";
            if (process.ExitCode != 0)
                result += $"{Environment.NewLine}Exit code: {process.ExitCode}";
            return string.IsNullOrWhiteSpace(result) ? "Git command completed with no output." : result;
        }

        public string GetStatus() => RunGitCommand("status");
    }
}
