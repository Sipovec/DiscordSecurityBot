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

public class ExportRolesModule(ICsvExportService exportService) : ModuleBase
{
    private readonly ICsvExportService _exportService = exportService;
    private static readonly string[] first = ["Role Name"];

    [SlashCommand("exportroles", "Экспорт ролей в csv.")]
    public async Task ExportRolesAsync()
    {
        SocketGuild guild = Context.Guild;
        List<SocketRole> roles = [.. guild.Roles.OrderByDescending(r => r.Position)];
        GuildPermission[] allPermissions = Constants.DiscordPermissions.UiPermissionOrder;

        // CSV format
        StringBuilder sb = new();

        IEnumerable<string> headerFields = first.Concat(allPermissions.Select(p => p.ToString()));
        sb.AppendLine(string.Join(",", headerFields.Select(CsvFormatter.FormatCsvField)));

        foreach (SocketRole role in roles)
        {
            List<string> fields =
            [
                role.Name, .. allPermissions.Select(perm => role.Permissions.Has(perm) ? "TRUE" : "FALSE")
            ];
            sb.AppendLine(string.Join(",", fields.Select(CsvFormatter.FormatCsvField)));
        }

        // Save file
        await _exportService.SaveCsvAsync(guild, "roles", sb.ToString());
    }
}
