using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Modules
{
    /// <summary>
    /// Модуль для получения информации о владельце гильдии.
    /// </summary>
    public class OwnerInfoModule : ModuleBase
    {
        [SlashCommand("owner", "Показывает информацию о владельце сервера.")]
        public async Task OwnerInfoAsync()
        {
            SocketGuild guild = Context.Guild;
            SocketGuildUser owner = guild.GetUser(guild.OwnerId);

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("Информация о владельце сервера")
                .WithColor(Color.Gold)
                .AddField("Имя", owner.Mention ?? owner.Username, true)
                .AddField("ID", owner.Id, true)
                .AddField("Присоединился", owner.JoinedAt?.ToString("dd.MM.yyyy HH:mm") ?? "Неизвестно", true)
                .AddField("Аккаунт создан", owner.CreatedAt.ToString("dd.MM.yyyy HH:mm"), true);

            // Вывод данных в embed сообщении
            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}
