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

        // Поиск ролей по имени (без учёта регистра)
        List<IRole> foundRoles = [];
        List<string> notFound = [];

        foreach (string name in roleNames)
        {
            // Убираем возможный символ '@' в начале, если пользователь его поставил
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
            string msg = "Не найдено ни одной роли. Проверьте названия.\n" +
                      $"Не найдены: `{string.Join("`, `", notFound)}`";
            await FollowupAsync(msg, ephemeral: true);
            return;
        }

        if (notFound.Count > 0)
        {
            await FollowupAsync(
                $"Следующие роли не найдены: `{string.Join("`, `", notFound)}`\n" +
                "Остальные роли будут обработаны.", ephemeral: true);
        }

        // Загрузка участников
        await guild.DownloadUsersAsync(); // Нужен GatewayIntents.GuildMembers
        IReadOnlyCollection<SocketGuildUser> allUsers = guild.Users;

        // Фильтрация по найденным ролям
        HashSet<ulong> targetRoleIds = [.. foundRoles.Select(r => r.Id)];
        List<SocketGuildUser> filteredUsers = [.. allUsers.Where(u => u.Roles.Any(r => targetRoleIds.Contains(r.Id)))];

        if (filteredUsers.Count == 0)
        {
            await FollowupAsync($"Участников с указанными ролями не найдено.", ephemeral: true);
            return;
        }

        // Формирование CSV
        StringBuilder sb = new();

        string[] headers = ["Username", "Display Name", "User ID", "Joined At", "Roles"];
        sb.AppendLine(string.Join(",", headers.Select(CsvFormatter.FormatCsvField)));

        foreach (SocketGuildUser? user in filteredUsers.OrderBy(u => u.Username))
        {
            string[] values =
            [
                user.Username,
                user.DisplayName ?? user.Username,
                user.Id.ToString(),
                user.JoinedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A",
                string.Join(", ", user.Roles.Where(r => r.Id != guild.EveryoneRole.Id).Select(r => r.Name)),
            ];
            sb.AppendLine(string.Join(",", values.Select(CsvFormatter.FormatCsvField)));
        }

        // Сохранение в файл
        await _exportService.SaveCsvAsync(guild, "users", sb.ToString());
    }
}
