using Newtonsoft.Json;
using SnaggingTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SnaggingTracker.Services
{
    /// <summary>
    /// Concrete implementation of IDataService using local JSON file storage.
    /// Data is persisted to %AppData%\SnaggingTracker\snags.json.
    /// </summary>
    public class JsonDataService : IDataService
    {
        private readonly string _dataDirectory;
        private readonly string _filePath;

        public JsonDataService()
        {
            _dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SnaggingTracker"
            );
            _filePath = Path.Combine(_dataDirectory, "snags.json");
        }

        public async Task<List<SnagIssue>> LoadIssuesAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<SnagIssue>();

                string json = await File.ReadAllTextAsync(_filePath);

                if (string.IsNullOrWhiteSpace(json))
                    return new List<SnagIssue>();

                var issues = JsonConvert.DeserializeObject<List<SnagIssue>>(json);
                return issues ?? new List<SnagIssue>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JsonDataService] Load error: {ex.Message}");
                return new List<SnagIssue>();
            }
        }

        public async Task SaveIssuesAsync(List<SnagIssue> issues)
        {
            try
            {
                if (!Directory.Exists(_dataDirectory))
                    Directory.CreateDirectory(_dataDirectory);

                string json = JsonConvert.SerializeObject(issues, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JsonDataService] Save error: {ex.Message}");
                throw;
            }
        }

        public string GenerateReferenceNumber(List<SnagIssue> existingIssues)
        {
            int next = (existingIssues?.Count ?? 0) + 1;
            return $"SNAG-{next:D3}";
        }
    }
}
