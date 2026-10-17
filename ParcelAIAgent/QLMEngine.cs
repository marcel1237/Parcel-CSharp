using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ParcelAIAgent
{
    public class QLMEngine
    {
        private readonly Dictionary<string, string> _rules = new();

        public void LoadRules(string jsonDirectory)
        {
            if (!Directory.Exists(jsonDirectory)) return;

            foreach (var file in Directory.GetFiles(jsonDirectory, "*.json"))
            {
                try
                {
                    var content = File.ReadAllText(file);
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    _rules[fileName] = content;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading QLM rule file {file}: {ex.Message}");
                }
            }
        }

        public string GetRule(string name)
        {
            return _rules.TryGetValue(name, out var rule) ? rule : "Rule not found.";
        }

        public int RuleCount => _rules.Count;
    }
}
