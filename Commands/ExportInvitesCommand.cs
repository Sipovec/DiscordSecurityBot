using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using DiscordSecurityBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Modules;

[RequireBotPermission(GuildPermission.ManageGuild)]
public class ExportInvitesCommand(IXlsxExportService exportService) : CommandBase
{
    private readonly IXlsxExportService _exportService = exportService;

    private static readonly IReadOnlyList<string> _headers =
    [
        "Код",
        "Канал",
        "Пригласивший",
        "Макс. использований",
        "Использовано",
        "Создан",
        "Истекает",
        "Временный"
    ];

    [SlashCommand("export_invites", "Экспорт приглашений в XLSX.")]
    public async Task ExportInvitesAsync()
    {
        SocketGuild guild = Context.Guild;

        // Need ManageGuild
        IReadOnlyCollection<RestInviteMetadata> invites = await guild.GetInvitesAsync();

        List<IReadOnlyList<object?>> rows = [];
        foreach (RestInviteMetadata invite in invites)
        {
            rows.Add(
            [
                invite.Code,
                invite.ChannelName,
                invite.Inviter,
                invite.MaxUses,
                invite.Uses,
                invite.CreatedAt,
                invite.ExpiresAt,
                invite.IsTemporary
            ]);
        }

        // Save file
        await _exportService.SaveXlsxAsync(guild, "invites", _headers, rows);

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }
}
