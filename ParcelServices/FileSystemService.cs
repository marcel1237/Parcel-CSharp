using System.IO;

namespace ParcelServices
{
    public class FileSystemService
    {
        public string[] GetFiles(string path, string searchPattern = "*.*")
        {
            if (!Directory.Exists(path)) return new string[0];
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteFile(string path, string content)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, content);
        }
    }
}
