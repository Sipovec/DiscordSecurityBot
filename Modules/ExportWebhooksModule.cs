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

[RequireBotPermission(GuildPermission.ManageWebhooks)]
public class ExportWebhooksModule(ICsvExportService exportService) : ModuleBase
{
    private readonly ICsvExportService _exportService = exportService;

    [SlashCommand("exportwebhooks", "Экспорт вебхуков в csv.")]
    public async Task ExportWebhooksAsync()
    {
        SocketGuild guild = Context.Guild;

        IReadOnlyCollection<IWebhook> webhooks = await guild.GetWebhooksAsync(); // Нужны права

        IOrderedEnumerable<IWebhook> sortedWebhooks = webhooks.OrderBy(w => w.Name ?? string.Empty);

        // Заголовки без ID
        string[] headers =
        [
            "Name",
            "Channel",
            "Type",
            "Creator",
            "Created At",
            "Has Token"
        ];

        StringBuilder sb = new();
        sb.AppendLine(string.Join(",", headers.Select(CsvFormatter.FormatCsvField)));

        foreach (IWebhook webhook in sortedWebhooks)
        {
            // Имя веб-хука
            string name = webhook.Name ?? string.Empty;

            // Имя канала (без ID)
            string channelName = "";
            if (webhook.ChannelId.HasValue)
            {
                SocketGuildChannel channel = guild.GetChannel(webhook.ChannelId.Value);
                if (channel != null)
                {
                    channelName = channel.Name;
                }
            }

            string typeStr = webhook.Type.ToString();

            // Имя создателя
            string creatorName = "";
            if (webhook.Creator != null)
            { 
                creatorName = webhook.Creator.Username; 
            }

            // Дата создания
            DateTimeOffset createdAt = SnowflakeUtils.FromSnowflake(webhook.Id);
            string createdAtStr = createdAt.ToString("dd.MM.yy HH:mm:ss UTC");

            // Наличие токена
            string hasTokenStr = string.IsNullOrEmpty(webhook.Token) ? "FALSE" : "TRUE";

            string[] fields =
            [
                name,
                channelName,
                typeStr,
                creatorName,
                createdAtStr,
                hasTokenStr
            ];

            sb.AppendLine(string.Join(",", fields.Select(CsvFormatter.FormatCsvField)));
        }

        // Сохранение файла
        await _exportService.SaveCsvAsync(guild, "webhooks", sb.ToString());
    }
}
