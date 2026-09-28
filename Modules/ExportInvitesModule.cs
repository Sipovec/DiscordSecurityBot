using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using DiscordSecurityBot.Helpers;
using DiscordSecurityBot.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Modules;

[RequireBotPermission(GuildPermission.ManageGuild)]
public class ExportInvitesModule(ICsvExportService exportService) : ModuleBase
{
    private readonly ICsvExportService _exportService = exportService;

    [SlashCommand("exportinvites", "Экспорт приглашений в csv.")]
    public async Task ExportInvitesAsync()
    {
        SocketGuild guild = Context.Guild;

        // Get all guild invates
        IReadOnlyCollection<RestInviteMetadata> invites = await guild.GetInvitesAsync(); // Need ManageGuild

        // CSV format
        StringBuilder csvBuilder = new();

        List<string> headers =
        [
            "Код",
            "Канал",
            "Пригласивший",
            "Макс. использований",
            "Использовано",
            "Создан",
            "Истекает",
            "Временный",
            "Ссылка"
        ];
        csvBuilder.AppendLine(string.Join(",", headers.Select(CsvFormatter.FormatCsvField)));

        foreach (RestInviteMetadata invite in invites)
        {
            // Get chanel name from ID
            IChannel channel = await Context.Client.GetChannelAsync(invite.ChannelId);
            string channelName = channel?.Name ?? "Неизвестно";

            // Invater
            IUser inviter = invite.Inviter;
            string inviterName = inviter?.Username ?? "";

            // Invites property
            CultureInfo invar = CultureInfo.InvariantCulture;

            string code = invite.Code;
            string maxUses = invite.MaxUses?.ToString(invar) ?? "Безлимит";
            string uses = invite.Uses?.ToString(invar) ?? "0";
            string createdAt = invite.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss", invar) ?? "";
            string expiresAt = invite.ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss", invar) ?? "Никогда";
            string isTemporary = invite.IsTemporary ? "TRUE" : "FALSE";
            string inviteUrl = $"https://discord.gg/{code}";

            List<string> fields =
            [
                code,
                channelName,
                inviterName,
                maxUses,
                uses,
                createdAt,
                expiresAt,
                isTemporary,
                inviteUrl
            ];
            csvBuilder.AppendLine(string.Join(",", fields.Select(CsvFormatter.FormatCsvField)));
        }

        // Save file
        await _exportService.SaveCsvAsync(guild, "invites", csvBuilder.ToString());

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }
}
