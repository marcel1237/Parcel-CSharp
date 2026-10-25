using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace ParcelServices
{
    public class PersistenceService
    {
        private readonly string _connectionString;

        public PersistenceService(string dbPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? ".");
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                );
                CREATE TABLE IF NOT EXISTS Sessions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionName TEXT,
                    Timestamp TEXT
                );
            ";
            command.ExecuteNonQuery();
        }

        public void SaveSetting(string key, string value)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "INSERT OR REPLACE INTO Settings (Key, Value) VALUES (@key, @value);";
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@value", value);
            command.ExecuteNonQuery();
        }

        public string? GetSetting(string key)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Value FROM Settings WHERE Key = @key;";
            command.Parameters.AddWithValue("@key", key);
            return command.ExecuteScalar()?.ToString();
        }

        public void SaveSession(string? workspacePath, string? openFilePath, int editorMode)
        {
            SaveSetting("WorkspacePath", workspacePath ?? string.Empty);
            SaveSetting("OpenFilePath", openFilePath ?? string.Empty);
            SaveSetting("EditorMode", editorMode.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public (string? WorkspacePath, string? OpenFilePath, int EditorMode) LoadSession()
        {
            var workspace = GetSetting("WorkspacePath");
            var file = GetSetting("OpenFilePath");
            var modeText = GetSetting("EditorMode");
            return (
                string.IsNullOrWhiteSpace(workspace) ? null : workspace,
                string.IsNullOrWhiteSpace(file) ? null : file,
                int.TryParse(modeText, out var mode) ? mode : 0);
        }

        public IReadOnlyList<string> ExecuteQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("SQL query is required.", nameof(sql));

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            using var reader = command.ExecuteReader();
            var rows = new List<string>();
            while (reader.Read())
            {
                var values = Enumerable.Range(0, reader.FieldCount)
                    .Select(index => $"{reader.GetName(index)}={reader.GetValue(index)}");
                rows.Add(string.Join(" | ", values));
            }
            return rows;
        }
    }
}
