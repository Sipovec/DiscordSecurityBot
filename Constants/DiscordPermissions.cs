using Discord;

namespace DiscordSecurityBot.Constants;

public static class DiscordPermissions
{
    public sealed record PermissionGroup(string Title, GuildPermission[] Permissions);

    public static readonly PermissionGroup[] UiGroups =
    [
        new("Основные права сервера", [
            GuildPermission.ViewChannel,
            GuildPermission.ManageChannels,
            GuildPermission.ManageRoles,
            GuildPermission.CreateGuildExpressions,
            GuildPermission.ManageEmojisAndStickers,
            GuildPermission.ViewAuditLog,
            GuildPermission.ViewGuildInsights,
            GuildPermission.ManageWebhooks,
            GuildPermission.ManageGuild,
            GuildPermission.ViewMonetizationAnalytics, // Только для серверов с монетизацией
        ]),
        new("Права участников", [
            GuildPermission.CreateInstantInvite,
            GuildPermission.ChangeNickname,
            GuildPermission.ManageNicknames,
            GuildPermission.KickMembers,
            GuildPermission.BanMembers,
            GuildPermission.ModerateMembers,
        ]),
        new("Права текстового канала", [
            GuildPermission.SendMessages,
            GuildPermission.SendMessagesInThreads,
            GuildPermission.CreatePublicThreads,
            GuildPermission.CreatePrivateThreads,
            GuildPermission.EmbedLinks,
            GuildPermission.AttachFiles,
            GuildPermission.AddReactions,
            GuildPermission.UseExternalEmojis,
            GuildPermission.UseExternalStickers,
            GuildPermission.MentionEveryone,
            GuildPermission.ManageMessages,
            GuildPermission.PinMessages,
            GuildPermission.BypassSlowmode,
            GuildPermission.ManageThreads,
            GuildPermission.ReadMessageHistory,
            GuildPermission.SendTTSMessages,
            GuildPermission.SendVoiceMessages,
            GuildPermission.SendPolls,
        ]),
        new("Права голосового канала", [
            GuildPermission.Connect,
            GuildPermission.Speak,
            GuildPermission.Stream,
            GuildPermission.UseSoundboard,
            GuildPermission.UseExternalSounds,
            GuildPermission.UseVAD,
            GuildPermission.PrioritySpeaker,
            GuildPermission.MuteMembers,
            GuildPermission.DeafenMembers,
            GuildPermission.MoveMembers,
            GuildPermission.SetVoiceChannelStatus,
        ]),
        new("Права приложений", [
            GuildPermission.UseApplicationCommands,
            GuildPermission.StartEmbeddedActivities,
            GuildPermission.UseExternalApps,
            GuildPermission.UseClydeAI, // Скрытое право, вроде нельзя изменить
        ]),
        new("Права для трибуны", [
            GuildPermission.RequestToSpeak,
        ]),
        new("Права доступа к событиям", [
            GuildPermission.CreateEvents,
            GuildPermission.ManageEvents,
        ]),
        new("Расширенные права", [
            GuildPermission.Administrator,
        ]),
    ];
}
