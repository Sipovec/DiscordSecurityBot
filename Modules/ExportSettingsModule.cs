using Discord;
using Discord.Interactions;
using DiscordSecurityBot.Helpers;
using DiscordSecurityBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiscordSecurityBot.Models;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordSecurityBot.Modules;

public class ExportGuildSettingsModule(ICsvExportService exportService) : ModuleBase
{
    private readonly ICsvExportService _exportService = exportService;
    private static readonly string[] sourceArray = ["Setting", "Value"];

    [SlashCommand("exportsettings", "Экспорт настроек сервера в csv.")]
    public async Task ExportSettingsAsync()
    {
        SocketGuild guild = Context.Guild;

        // Сбор всех фич сервера
        List<string> featureStrings = [];
        IEnumerable<GuildFeature> allFeatureValues = Enum.GetValues<GuildFeature>().Cast<GuildFeature>();
        foreach (GuildFeature f in allFeatureValues)
        {
            if (guild.Features.HasFeature(f))
            {
                featureStrings.Add(f.ToString());
            }
        }
        featureStrings.AddRange(guild.Features.Experimental);
        string featuresStr = featureStrings.Count != 0 ? string.Join(", ", featureStrings) : "None";

        GuildSettings settings = new(guild);

        // Формирование CSV
        StringBuilder sb = new();
        sb.AppendLine(string.Join(",", sourceArray.Select(CsvFormatter.FormatCsvField)));
        foreach (var kvp in settings.Properties)
            sb.AppendLine(string.Join(",", new[] { kvp.Key, kvp.Value }.Select(CsvFormatter.FormatCsvField)));

        // Сохранение файла
        await _exportService.SaveCsvAsync(guild, "settings", sb.ToString());
    }
}
