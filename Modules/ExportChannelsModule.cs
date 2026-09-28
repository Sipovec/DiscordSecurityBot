using Discord.Interactions;
using Discord.WebSocket;
using DiscordSecurityBot.Helpers;
using DiscordSecurityBot.Models;
using DiscordSecurityBot.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Modules;

public class ExportChannelsModule(ICsvExportService exportService) : ModuleBase
{
    private readonly ICsvExportService _exportService = exportService;

    [SlashCommand("exportchannels", "Экспорт каналов в csv.")]
    public async Task ExportChannelsAsync()
    {
        SocketGuild guild = Context.Guild;

        // DTO
        List<ChannelRecord> records = [.. guild.Channels.Select(channel => new ChannelRecord(channel, guild))];

        // Формирование CSV
        StringBuilder sb = new();

        string[] headers =
        [
            "ChannelName", "ChannelType", "CategoryName", "Topic", 
            "NSFW", "SlowMode", "Bitrate", "UserLimit", "OverwritesJson"
        ];
        sb.AppendLine(string.Join(",", headers.Select(CsvFormatter.FormatCsvField)));

        CultureInfo invar = CultureInfo.InvariantCulture;

        foreach (ChannelRecord rec in records)
        {
            string[] values =
            [
                rec.ChannelName,
                rec.ChannelType,
                rec.CategoryName,
                rec.Topic,
                rec.Nsfw.ToString(),
                rec.SlowMode.ToString(invar),
                rec.Bitrate.ToString(invar),
                rec.UserLimit.ToString(invar),
                rec.OverwritesJson
            ];
            sb.AppendLine(string.Join(",", values.Select(CsvFormatter.FormatCsvField)));
        }

        await _exportService.SaveCsvAsync(guild, "channels", sb.ToString());

        await FollowupAsync("Команда успешно выполнена.", ephemeral: true);
    }
}
