using System.Collections.ObjectModel;

namespace ParcelServices;

public sealed record WorkspaceEntry(string FullPath, string RelativePath, bool IsDirectory);

public sealed class WorkspaceService
{
    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", "bin", "obj", ".vs", ".idea"
    };

    public string? RootPath { get; private set; }

    public string OpenOrCreate(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("A workspace path is required.", nameof(path));

        var fullPath = Path.GetFullPath(path.Trim());
        Directory.CreateDirectory(fullPath);
        RootPath = fullPath;
        return fullPath;
    }

    public IReadOnlyList<WorkspaceEntry> GetEntries()
    {
        if (RootPath is null || !Directory.Exists(RootPath))
            return Array.Empty<WorkspaceEntry>();

        var entries = new List<WorkspaceEntry>();
        AddEntries(RootPath, entries);
        return new ReadOnlyCollection<WorkspaceEntry>(entries);
    }

    public string ResolvePath(string relativeOrFullPath)
    {
        if (RootPath is null)
            throw new InvalidOperationException("Open a workspace before accessing files.");
        if (string.IsNullOrWhiteSpace(relativeOrFullPath))
            throw new ArgumentException("A file path is required.", nameof(relativeOrFullPath));

        var candidate = Path.GetFullPath(
            Path.IsPathRooted(relativeOrFullPath)
                ? relativeOrFullPath
                : Path.Combine(RootPath, relativeOrFullPath));
        var root = RootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(candidate, RootPath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The selected path is outside the workspace.");
        return candidate;
    }

    private static void AddEntries(string directory, ICollection<WorkspaceEntry> entries)
    {
        foreach (var child in Directory.EnumerateDirectories(directory)
                     .Where(path => !IgnoredDirectories.Contains(Path.GetFileName(path)))
                     .OrderBy(path => path))
        {
            entries.Add(new WorkspaceEntry(child, Path.GetRelativePath(directory, child), true));
            AddEntries(child, entries);
        }

        foreach (var file in Directory.EnumerateFiles(directory).OrderBy(path => path))
            entries.Add(new WorkspaceEntry(file, Path.GetRelativePath(directory, file), false));
    }
}
