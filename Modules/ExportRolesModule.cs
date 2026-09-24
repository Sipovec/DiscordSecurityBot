using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordSecurityBot.Constants;
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

    [SlashCommand("exportroles", "Экспорт ролей в csv.")]
    public async Task ExportRolesAsync()
    {
        SocketGuild guild = Context.Guild;
        List<SocketRole> roles = [.. guild.Roles.OrderByDescending(r => r.Position)];

        DiscordPermissions.PermissionGroup[] groups = DiscordPermissions.UiGroups;

        StringBuilder sb = new();

        // Group names
        List<string> groupRow = [""];
        foreach (DiscordPermissions.PermissionGroup group in groups)
        {
            groupRow.Add(group.Title);
            for (int i = 1; i < group.Permissions.Length; i++)
            {
                groupRow.Add("");
            }
        }
        sb.AppendLine(string.Join(",", groupRow.Select(CsvFormatter.FormatCsvField)));

        // Permission names
        List<string> permRow = ["Role Name"];
        foreach (DiscordPermissions.PermissionGroup group in groups)
        {
            foreach (GuildPermission perm in group.Permissions)
            {
                permRow.Add(perm.ToString());
            }
        }
        sb.AppendLine(string.Join(",", permRow.Select(CsvFormatter.FormatCsvField)));

        // Data rows
        foreach (SocketRole role in roles)
        {
            List<string> row = [role.Name];
            foreach (DiscordPermissions.PermissionGroup group in groups)
            {
                foreach (GuildPermission perm in group.Permissions)
                { 
                    row.Add(role.Permissions.Has(perm) ? "TRUE" : "FALSE");
                }
            }
            sb.AppendLine(string.Join(",", row.Select(CsvFormatter.FormatCsvField)));
        }

        // Save file
        await _exportService.SaveCsvAsync(guild, "roles", sb.ToString());

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }
}
