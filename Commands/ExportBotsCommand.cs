using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using DiscordSecurityBot.Constants;
using DiscordSecurityBot.Helpers;
using DiscordSecurityBot.Services;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Modules;

[RequireBotPermission(GuildPermission.ManageGuild)]
public class ExportBotsCommand(ICsvExportService exportService) : CommandBase
{
    private readonly ICsvExportService _exportService = exportService;

    [SlashCommand("export_bots", "Export bots in csv.")]
    public async Task ExportIntegrationsAsync()
    {
        SocketGuild guild = Context.Guild;

        IReadOnlyCollection<IIntegration> integrations = await guild.GetIntegrationsAsync(); // Need ManageGuild

        ImmutableArray <PermissionGroup> groups = DiscordPermissions.UiGroups;

        StringBuilder sb = new();

        // Group names
        List<string> groupRow = [""];
        foreach (PermissionGroup group in groups)
        {
            groupRow.Add(group.Title);
            for (int i = 1; i < group.Permissions.Length; i++)
            {
                groupRow.Add("");
            }
        }
        sb.AppendLine(string.Join(",", groupRow.Select(CsvFormatter.FormatCsvField)));

        // Permission names
        List<string> permRow = ["Bot"];
        foreach (PermissionGroup group in groups)
        {
            foreach (GuildPermission perm in group.Permissions)
            {
                permRow.Add(perm.ToString());
            }
        }
        sb.AppendLine(string.Join(",", permRow.Select(CsvFormatter.FormatCsvField)));

        // Data rows
        DiscordSocketRestClient restClient = Context.Client.Rest;
        foreach (IIntegration integration in integrations)
        {
            IUser? botUser = integration.Application?.Bot;
            if (botUser is not { IsBot: true }) continue;

            // REST don't need Server Members Intent
            RestGuildUser? botGuildUser = await restClient.GetGuildUserAsync(guild.Id, botUser.Id);

            ulong combinedRaw = guild.EveryoneRole.Permissions.RawValue;
            Dictionary<ulong, SocketRole> rolesById = guild.Roles.ToDictionary(r => r.Id);
            foreach (ulong roleId in botGuildUser.RoleIds)
            {
                if (rolesById.TryGetValue(roleId, out SocketRole? role))
                {
                    // Combine bit flags
                    combinedRaw |= role.Permissions.RawValue;
                }
            }
            GuildPermissions permissions = new(combinedRaw);

            List<string> row = [integration.Name];
            foreach (PermissionGroup group in groups)
            {
                foreach (GuildPermission perm in group.Permissions)
                {
                    row.Add(permissions.Has(perm) ? "TRUE" : "FALSE");
                }
            }
            sb.AppendLine(string.Join(",", row.Select(CsvFormatter.FormatCsvField)));
        }

        // Save file
        await _exportService.SaveCsvAsync(guild, "integration", sb.ToString());

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }
}
