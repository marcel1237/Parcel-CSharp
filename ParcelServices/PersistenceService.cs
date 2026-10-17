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
    }
}
