using Discord;
using Discord.Interactions;

namespace DiscordSecurityBot.Modules
{
    // Доступы команд
    [RequireContext(ContextType.Guild)]                        // Только в гильдии
    [DefaultMemberPermissions(GuildPermission.Administrator)]  // Видны только админам (клиентская проверка)
    [RequireUserPermission(GuildPermission.Administrator)]     // Выполняются только админами (серверная проверка)
    public abstract class ModuleBase : InteractionModuleBase<SocketInteractionContext>
    {

    }
}
