using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordSecurityBot.Helpers;
using DiscordSecurityBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Modules;

public class ExportUsersModule(ICsvExportService exportService) : ModuleBase
{
    private readonly ICsvExportService _exportService = exportService;

    [SlashCommand("exportusers", "Экспорт участников с указанными ролями в csv.")]
    public async Task ExportUsersAsync(
        [Summary(description: "Названия ролей через запятую")] string rolesInput
    )
    {
        SocketGuild guild = Context.Guild;

        // Разбор ввода: разделитель - запятая
        if (string.IsNullOrWhiteSpace(rolesInput))
        {
            await FollowupAsync("Укажите хотя бы одну роль.", ephemeral: true);
            return;
        }

        List<string> roleNames = [.. rolesInput.Split([','], StringSplitOptions.RemoveEmptyEntries)
                                  .Select(s => s.Trim())
                                  .Where(s => !string.IsNullOrEmpty(s))
                                  ];

        if (roleNames.Count == 0)
        {
            await FollowupAsync("Не указано ни одной роли.", ephemeral: true);
            return;
        }

        List<IRole> foundRoles = [];
        List<string> notFound = [];

        foreach (string name in roleNames)
        {
            // Del '@' symbol
            string cleanName = name.StartsWith('@') ? name[1..] : name;

            SocketRole? role = guild.Roles.FirstOrDefault(r =>
                r.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase));

            if (role != null)
            {
                foundRoles.Add(role);
            }
            else
            {
                notFound.Add(name);
            }
        }

        if (foundRoles.Count == 0)
        {
            await FollowupAsync("Roles not found", ephemeral: true);
            return;
        }

        if (notFound.Count > 0)
        {
            await FollowupAsync($"Roles not found: `{string.Join("`, `", notFound)}`", ephemeral: true);
        }

        // Get users for role
        HashSet<ulong> targetRoleIds = [.. foundRoles.Select(r => r.Id)];
        List<IGuildUser> filteredUsers = await SearchMembersByRolesAsync(guild, targetRoleIds);

        if (filteredUsers.Count == 0)
        {
            await FollowupAsync($"No users with the specified roles were found.", ephemeral: true);
            return;
        }

        // CSV format
        StringBuilder sb = new();

        string[] headers = ["Username", "Display Name", "User ID", "Joined At", "Roles"];
        sb.AppendLine(string.Join(",", headers.Select(CsvFormatter.FormatCsvField)));

        foreach (IGuildUser user in filteredUsers.OrderBy(u => u.Username))
        {
            List<string?> roleNamesList = 
                [.. user.RoleIds
                .Where(id => id != guild.EveryoneRole.Id)
                .Select(id => guild.GetRole(id)?.Name)
                .Where(name => name != null)
                ];

            string[] values =
            [
                user.Username,
                user.DisplayName ?? "",
                user.Id.ToString(),
                user.JoinedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A",
                string.Join(", ", roleNamesList),
            ];
            sb.AppendLine(string.Join(",", values.Select(CsvFormatter.FormatCsvField)));
        }

        // Save file
        await _exportService.SaveCsvAsync(guild, "users", sb.ToString());

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }

    /// <summary>
    /// Возвращает список пользователей с указанными ролями.
    /// </summary>
    /// <param name="guild"></param>
    /// <param name="roleIds"></param>
    /// <returns></returns>
    private static async Task<List<IGuildUser>> SearchMembersByRolesAsync(SocketGuild guild, HashSet<ulong> roleIds)
    {
        List<IGuildUser> allUsers = [];
        const int pageSize = 1000;

        // Базовый фильтр: участник должен иметь хотя бы одну из указанных ролей (OR)
        MemberSearchSnowflakeQuery roleQuery = new()
        {
            OrQuery = [.. roleIds]
        };

        MemberSearchFilter filter = new()
        {
            RoleIds = roleQuery
        };

        MemberSearchPropertiesV2 properties = new()
        {
            OrQuery = filter
        };

        MemberSearchPaginationFilter? after = null;

        while (true)
        {
            if (after != null)
            { 
                properties.After = after; 
            }

            MemberSearchResult result;
            try
            {
                // Не требует GuildMembers intent, в отличие от DownloadUsersAsync()
                result = await guild.SearchUsersAsyncV2(pageSize, properties);
            }
            catch
            {
                break;
            }

            if (result.Members.Count == 0)
            {
                break;
            }

            allUsers.AddRange(result.Members.Select(m => m.User));

            if (result.Members.Count < pageSize)
            {
                break;
            }

            // Курсор для следующей страницы: последний пользователь текущей страницы
            IGuildUser lastUser = result.Members.Last().User;
            after = new MemberSearchPaginationFilter(lastUser.Id, lastUser.JoinedAt ?? DateTimeOffset.MinValue);
        }

        return allUsers;
    }
}
