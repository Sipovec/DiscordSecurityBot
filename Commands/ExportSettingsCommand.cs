using Discord.Interactions;
using DiscordSecurityBot.Services;
using System.Collections.Generic;
using DiscordSecurityBot.Models;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordSecurityBot.Modules;

public class ExportGuildSettingsModule(IXlsxExportService exportService) : CommandBase
{
    private readonly IXlsxExportService _exportService = exportService;
    private static readonly IReadOnlyList<string> _headers = ["Setting", "Value"];

    [SlashCommand("export_settings", "Экспорт настроек сервера в XLSX.")]
    public async Task ExportSettingsAsync()
    {
        SocketGuild guild = Context.Guild;

        GuildSettings settings = new(guild);

        List<IReadOnlyList<object?>> rows = [];
        foreach (KeyValuePair<string, string> kvp in settings.Properties)
        {
            rows.Add([kvp.Key, kvp.Value]);
        }

        // Save file
        await _exportService.SaveXlsxAsync(guild, "settings", _headers, rows);

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }
}
