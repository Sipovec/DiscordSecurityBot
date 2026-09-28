using Discord;
using System.Collections.Immutable;

namespace DiscordSecurityBot.Constants;

public sealed record PermissionGroup(string Title, ImmutableArray<GuildPermission> Permissions);
