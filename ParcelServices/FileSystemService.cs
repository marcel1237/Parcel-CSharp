using System.IO;

namespace ParcelServices
{
    public class FileSystemService
    {
        public string[] GetFiles(string path, string searchPattern = "*.*")
        {
            if (!Directory.Exists(path)) return new string[0];
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories)
                .Where(file => !file.Split(Path.DirectorySeparatorChar)
                    .Any(part => part is ".git" or "bin" or "obj" or ".vs" or ".idea"))
                .OrderBy(file => file)
                .ToArray();
        }

        public string ReadFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("The selected file does not exist.", path);
            return File.ReadAllText(path);
        }

        public void WriteFile(string path, string content)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, content ?? string.Empty);
        }
    }
}
