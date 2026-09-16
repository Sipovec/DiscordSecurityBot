using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Discord.WebSocket;

namespace DiscordSecurityBot.Models
{
    public class GuildSettings
    {
        public Dictionary<string, string> Properties { get; }

        public GuildSettings(SocketGuild guild)
        {
            // Предварительно собираем фичи
            string featuresStr = string.Join(", ", guild.Features.Value);

            Properties = new Dictionary<string, string>
            {
                // Security & moderation
                ["Verification Level"] = guild.VerificationLevel.ToString(),
                ["Default Message Notifications"] = guild.DefaultMessageNotifications.ToString(),
                ["Explicit Content Filter"] = guild.ExplicitContentFilter.ToString(),
                ["MFA Level"] = guild.MfaLevel.ToString(),

                // Channels
                ["AFK Channel"] = guild.AFKChannel != null ? $"{guild.AFKChannel.Name} (ID: {guild.AFKChannel.Id})" : "None",
                ["AFK Timeout (seconds)"] = guild.AFKTimeout.ToString(),
                ["System Channel"] = guild.SystemChannel != null ? $"{guild.SystemChannel.Name} (ID: {guild.SystemChannel.Id})" : "None",
                ["System Channel Flags"] = guild.SystemChannelFlags.ToString(),
                ["Rules Channel"] = guild.RulesChannel != null ? $"{guild.RulesChannel.Name} (ID: {guild.RulesChannel.Id})" : "None",
                ["Public Updates Channel"] = guild.PublicUpdatesChannel != null ? $"{guild.PublicUpdatesChannel.Name} (ID: {guild.PublicUpdatesChannel.Id})" : "None",

                // Boosts / Premium
                ["Premium Tier"] = guild.PremiumTier.ToString(),
                ["Premium Subscription Count"] = guild.PremiumSubscriptionCount.ToString(),

                // Locale
                ["Preferred Locale"] = guild.PreferredLocale,

                // Features
                ["Features"] = featuresStr,

                // Vanity URL
                ["Vanity URL Code"] = guild.VanityURLCode ?? "None",

                // Images / URLs
                ["Splash URL"] = guild.SplashUrl ?? "None",
                ["Banner URL"] = guild.BannerUrl ?? "None",
                ["Discovery Splash URL"] = guild.DiscoverySplashUrl ?? "None",

                // Emojis
                ["Emojis"] = guild.Emotes.Count > 0
                    ? JsonSerializer.Serialize(guild.Emotes.Select(e => new { e.Name, e.Id, e.Animated, e.Url }))
                    : "None",

                // Stickers
                ["Stickers"] = guild.Stickers.Count > 0
                    ? JsonSerializer.Serialize(guild.Stickers.Select(s => new { s.Name, s.Id, s.Description, s.Format, s.Tags }))
                    : "None",

                // Counts
                ["Roles Count"] = guild.Roles.Count.ToString(),
                ["Channels Count"] = guild.Channels.Count.ToString(),
                ["Voice Channels Count"] = guild.VoiceChannels.Count.ToString(),
                ["Text Channels Count"] = guild.TextChannels.Count.ToString(),
                ["Categories Count"] = guild.CategoryChannels.Count.ToString()
            };
        }
    }
}
