using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordSecurityBot.Helpers;
using DiscordSecurityBot.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Modules;

/// <summary>
/// Модуль для экспорта ботов в CSV.
/// </summary>
public class ExportBotsModule(ICsvExportService exportService) : ModuleBase
{
    private readonly ICsvExportService _exportService = exportService;
    private static readonly string[] first = ["Bot Name"];

    [SlashCommand("exportbots", "Экспорт ботов в CSV.")]
    public async Task ExportBotsAsync()
    {
        SocketGuild guild = Context.Guild;
        List<SocketGuildUser> botUsers = [.. guild.Users.Where(u => u.IsBot)];

        if (botUsers.Count == 0)
        {
            await FollowupAsync("На этом сервере нет ботов.", ephemeral: true);
            return;
        }

        GuildPermission[] allPermissions = Constants.DiscordPermissions.UiPermissionOrder;

        // Формирование CSV
        StringBuilder sb = new();

        IEnumerable<string> headerFields = first.Concat(allPermissions.Select(p => p.ToString()));
        sb.AppendLine(string.Join(",", headerFields.Select(CsvFormatter.FormatCsvField)));

        foreach (SocketGuildUser bot in botUsers)
        {
            List<string> fields =
            [
                bot.Nickname ?? bot.Username,
                .. allPermissions.Select(perm => bot.GuildPermissions.Has(perm) ? "TRUE" : "FALSE"),
            ];
            sb.AppendLine(string.Join(",", fields.Select(CsvFormatter.FormatCsvField)));
        }

        // Сохранение файла
        await _exportService.SaveCsvAsync(guild, "bots", sb.ToString());

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }
}
