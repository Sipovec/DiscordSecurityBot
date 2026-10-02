using Discord;

namespace DiscordSecurityBot.Helpers;

public static class ChannelPermissionExtensions
{
    // In ChannelPermission miss Administrator
    private const ulong AdministratorBit = 1L << 3;

    public static string GetName(this ChannelPermission permission)
        => (ulong)permission == AdministratorBit
            ? "Administrator"
            : permission.ToString();
}
