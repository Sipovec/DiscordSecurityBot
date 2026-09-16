using Discord;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Services
{
    public interface ICsvExportService
    {
        /// <summary>
        /// Save CSV-string into file.
        /// </summary>
        /// <param name="guild">Guild name.</param>
        /// <param name="prefix">File name prefix ("channels", "roles", "users"... etc.).</param>
        /// <param name="csvContent">String</param>
        /// <returns>File path</returns>
        Task<string> SaveCsvAsync(IGuild guild, string prefix, string csvContent);
    }

    public class CsvExportService(ILogger<CsvExportService> logger) : ICsvExportService
    {
        private readonly ILogger<CsvExportService> _logger = logger;

        public async Task<string> SaveCsvAsync(IGuild guild, string prefix, string csvContent)
        {
            // Create Exports folder
            string exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
            Directory.CreateDirectory(exportDir);

            // Filter symbols in guild name
            string safeGuildName = string.Concat(guild.Name.Split(Path.GetInvalidFileNameChars()));
            if (string.IsNullOrWhiteSpace(safeGuildName))
            {
                safeGuildName = guild.Id.ToString();
            }

            // File name
            string fileName = $"{prefix}_{safeGuildName}_{DateTime.UtcNow:dd.MM.yy-HH.mm.ss}.csv";
            string filePath = Path.Combine(exportDir, fileName);

            // Write file
            await File.WriteAllTextAsync(filePath, csvContent, Encoding.UTF8);

            _logger.LogInformation("Save: {FilePath}", filePath);
            return filePath;
        }
    }
}
