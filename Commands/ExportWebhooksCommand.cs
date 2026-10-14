using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordSecurityBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Modules;

[RequireBotPermission(GuildPermission.ManageWebhooks)]
public class ExportWebhooksCommand(IXlsxExportService exportService) : CommandBase
{
    private readonly IXlsxExportService _exportService = exportService;

    private static readonly IReadOnlyList<string> _headers =
    [
        "Name",
        "Channel",
        "Type",
        "Creator",
        "Created At",
        "Has Token"
    ];

    [SlashCommand("export_webhooks", "Экспорт вебхуков в XLSX.")]
    public async Task ExportWebhooksAsync()
    {
        SocketGuild guild = Context.Guild;

        // Need ManageWebhooks
        IReadOnlyCollection<IWebhook> webhooks = await guild.GetWebhooksAsync();

        List<IReadOnlyList<object?>> rows = [];
        foreach (IWebhook webhook in webhooks)
        {
            string channelName = webhook.ChannelId is { } channelId
                ? guild.GetChannel(channelId)?.Name ?? ""
                : "";

            rows.Add(
            [
                webhook.Name ?? "",
                channelName,
                webhook.Type.ToString(),
                webhook.Creator?.Username ?? "",
                webhook.CreatedAt.UtcDateTime,
                !string.IsNullOrEmpty(webhook.Token)
            ]);
        }

        // Save file
        await _exportService.SaveXlsxAsync(guild, "webhooks", _headers, rows);

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }
}
