using System;
using System.IO;

namespace ParcelServices
{
    public class BackupService
    {
        private readonly string _backupDirectory;

        public BackupService(string backupDirectory)
        {
            _backupDirectory = backupDirectory;
            Directory.CreateDirectory(_backupDirectory);
        }

        public string CreateBackup(string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Source file not found", sourceFilePath);

            var fileName = Path.GetFileName(sourceFilePath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"{fileName}.{timestamp}.bak";
            var destPath = Path.Combine(_backupDirectory, backupFileName);

            File.Copy(sourceFilePath, destPath, true);
            return destPath;
        }
    }
}
