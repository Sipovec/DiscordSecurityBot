using Discord;
using System;
using System.Linq;

namespace DiscordSecurityBot.Constants
{
    public static class DiscordPermissions
    {
        private static readonly GuildPermission[] UiOrderRaw =
        [
            // Основные права сервера
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

            // Права участников
            GuildPermission.CreateInstantInvite,
            GuildPermission.ChangeNickname,
            GuildPermission.ManageNicknames,
            GuildPermission.KickMembers,
            GuildPermission.BanMembers,
            GuildPermission.ModerateMembers,

            // Права текстового канала
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

            // Права голосового канала
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
            
            // Права приложений
            GuildPermission.UseApplicationCommands,
            GuildPermission.StartEmbeddedActivities,
            GuildPermission.UseExternalApps,
            GuildPermission.UseClydeAI, // Скрытое право, вроде нельзя получить

            // Права для трибуны
            GuildPermission.RequestToSpeak,

            // Права доступа к событиям
            GuildPermission.CreateEvents,
            GuildPermission.ManageEvents,

            // Расширенные права
            GuildPermission.Administrator
        ];

        // В конец дописывает права неучтённые в списке
        public static readonly GuildPermission[] UiPermissionOrder = [.. UiOrderRaw, .. Enum.GetValues<GuildPermission>().Except(UiOrderRaw)];
    }
}
