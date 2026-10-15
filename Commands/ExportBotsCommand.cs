using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using DiscordSecurityBot.Constants;
using DiscordSecurityBot.Services;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Modules;

[RequireBotPermission(GuildPermission.ManageGuild)]
public class ExportBotsCommand(IXlsxExportService exportService) : CommandBase
{
    private readonly IXlsxExportService _exportService = exportService;

    [SlashCommand("export_bots", "Экспорт ботов в XLSX.")]
    public async Task ExportBotsAsync()
    {
        SocketGuild guild = Context.Guild;

        // Requires ManageGuild
        IReadOnlyCollection<IIntegration> integrations = await guild.GetIntegrationsAsync();

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

        List<IReadOnlyList<object?>> rows = [];

        // Permission names
        List<object?> permissionRow = ["Bot"];
        foreach (PermissionGroup group in groups)
        {
            foreach (GuildPermission permission in group.Permissions)
            {
                permissionRow.Add(permission.ToString());
            }
        }
        rows.Add(permissionRow);

        // Bot data
        DiscordSocketRestClient restClient = Context.Client.Rest;
        Dictionary<ulong, SocketRole> rolesById = guild.Roles.ToDictionary(role => role.Id);

        foreach (IIntegration integration in integrations)
        {
            IUser? botUser = integration.Application?.Bot;
            if (botUser is not { IsBot: true })
            {
                continue;
            }

            // REST doesn't require the Server Members Intent
            RestGuildUser? botGuildUser = await restClient.GetGuildUserAsync(guild.Id, botUser.Id);
            if (botGuildUser is null)
            {
                continue;
            }

            ulong combinedRaw = guild.EveryoneRole.Permissions.RawValue;
            foreach (ulong roleId in botGuildUser.RoleIds)
            {
                if (rolesById.TryGetValue(roleId, out SocketRole? role))
                {
                    combinedRaw |= role.Permissions.RawValue;
                }
            }

            GuildPermissions permissions = new(combinedRaw);
            List<object?> row = [integration.Name];
            foreach (PermissionGroup group in groups)
            {
                foreach (GuildPermission permission in group.Permissions)
                {
                    row.Add(permissions.Has(permission));
                }
            }
            rows.Add(row);
        }

        // Save file
        await _exportService.SaveXlsxAsync(guild, "bots", headers, rows);

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }
}
