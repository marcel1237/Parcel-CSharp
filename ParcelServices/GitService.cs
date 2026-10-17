using System.Diagnostics;

namespace ParcelServices
{
    public class GitService
    {
        private readonly string _repoPath;

        public GitService(string repoPath)
        {
            _repoPath = repoPath;
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

            return string.IsNullOrEmpty(error) ? output : $"{output}\nError: {error}";
        }

        public string GetStatus() => RunGitCommand("status");
    }
}
