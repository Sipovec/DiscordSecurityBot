using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordSecurityBot.Constants;
using DiscordSecurityBot.Services;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Modules;

public class ExportRolesCommand(IXlsxExportService exportService) : CommandBase
{
    private readonly IXlsxExportService _exportService = exportService;

    [SlashCommand("export_roles", "Экспорт ролей в XLSX.")]
    public async Task ExportRolesAsync()
    {
        SocketGuild guild = Context.Guild;

        IEnumerable<SocketRole> roles = guild.Roles.OrderByDescending(r => r.Position);

        ImmutableArray<PermissionGroup> groups = DiscordPermissions.UiGroups;

        // Groups
        List<string> headers = [""];
        foreach (PermissionGroup group in groups)
        {
            headers.Add(group.Title);

            for (int i = 1; i < group.Permissions.Length; i++)
            {
                headers.Add("");
            }
        }
        headers.Add("");

        List<IReadOnlyList<object?>> rows = [];

        // Permission names
        List<object?> permissionRow = [""];
        foreach (PermissionGroup group in groups)
        {
            foreach (GuildPermission permission in group.Permissions)
            {
                permissionRow.Add(permission.ToString());
            }
        }
        permissionRow.Add("Is Bot");
        rows.Add(permissionRow);

        // Data
        foreach (SocketRole role in roles)
        {
            List<object?> row = [role.Name];

            foreach (PermissionGroup group in groups)
            {
                foreach (GuildPermission permission in group.Permissions)
                {
                    row.Add(role.Permissions.Has(permission));
                }
            }
            row.Add(role.Tags.BotId.HasValue);
            rows.Add(row);
        }

        // Save file
        await _exportService.SaveXlsxAsync(guild, "roles", headers, rows);

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }
}
