using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Discord;
using Discord.WebSocket;

namespace DiscordSecurityBot.Models;

public class ChannelRecord
{
    public string ChannelName { get; }
    public string ChannelType { get; }
    public string CategoryName { get; }
    public string Topic { get; } = "";
    public bool Nsfw { get; }
    public int SlowMode { get; }
    public int Bitrate { get; }
    public int UserLimit { get; }
    public string OverwritesJson { get; } = "";

    public ChannelRecord(SocketGuildChannel channel, SocketGuild guild)
    {
        ChannelName = channel.Name;
        ChannelType = channel.GetChannelType().ToString() ?? "Unknown";
        CategoryName = channel is INestedChannel { CategoryId: ulong catId }
            ? guild.GetChannel(catId)?.Name ?? ""
            : "";

        // Specific fields
        switch (channel)
        {
            case SocketVoiceChannel voiceChannel:
                Bitrate = voiceChannel.Bitrate;
                UserLimit = voiceChannel.UserLimit ?? 0;
                break;

            case SocketTextChannel textChannel:
                Topic = textChannel.Topic ?? "";
                Nsfw = textChannel.IsNsfw;

                // News-channels don't support slow mode (NotSupportedException)
                SlowMode = textChannel is SocketNewsChannel
                    ? 0
                    : textChannel.SlowModeInterval;
                break;
        }

        // Overwrites
        if (channel.PermissionOverwrites.Count > 0)
        {
            IEnumerable<OverwriteRecord> overwriteRecords = channel.PermissionOverwrites.Select(overwrite =>
            {
                string targetName;
                if (overwrite.TargetType == PermissionTarget.Role)
                {
                    SocketRole role = guild.GetRole(overwrite.TargetId);
                    targetName = role?.Name ?? overwrite.TargetId.ToString();
                }
                else
                {
                    SocketGuildUser? user = guild.GetUser(overwrite.TargetId);
                    targetName = user?.DisplayName ?? user?.Username ?? overwrite.TargetId.ToString();
                }

                return new OverwriteRecord
                {
                    TargetName = targetName,
                    TargetType = overwrite.TargetType == PermissionTarget.Role ? "Role" : "User",
                    Allow = [.. overwrite.Permissions.ToAllowList().Select(p => p.ToString())],
                    Deny = [.. overwrite.Permissions.ToDenyList().Select(p => p.ToString())]
                };
            });

            OverwritesJson = JsonSerializer.Serialize(overwriteRecords);
        }
    }
}
