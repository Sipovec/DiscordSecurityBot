using Discord.Interactions;
using Discord.WebSocket;
using DiscordSecurityBot.Models;
using DiscordSecurityBot.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Modules;

public class ExportChannelsCommand(IXlsxExportService exportService) : CommandBase
{
    private readonly IXlsxExportService _exportService = exportService;

    private static readonly IReadOnlyList<string> _headers =
    [
        "ChannelName", "ChannelType", "CategoryName", "Topic",
        "NSFW", "SlowMode", "Bitrate", "UserLimit", "OverwritesJson"
    ];

    [SlashCommand("export_channels", "Экспорт каналов в XLSX.")]
    public async Task ExportChannelsAsync()
    {
        SocketGuild guild = Context.Guild;

        // DTO
        List<ChannelRecord> records = [.. guild.Channels.Select(channel => new ChannelRecord(channel, guild))];

        List<IReadOnlyList<object?>> rows = [];
        foreach (ChannelRecord rec in records)
        {
            rows.Add(
            [
                rec.ChannelName,
                rec.ChannelType,
                rec.CategoryName,
                rec.Topic,
                rec.Nsfw,
                rec.SlowMode,
                rec.Bitrate,
                rec.UserLimit,
                rec.OverwritesJson
            ]);
        }

        // Save file
        await _exportService.SaveXlsxAsync(guild, "channels", _headers, rows);

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }
}
