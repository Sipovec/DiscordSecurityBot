using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordSecurityBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using DiscordSecurityBot.Models;

namespace DiscordSecurityBot;
class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        try
        {
            using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                IConfiguration config = context.Configuration;
                IConfigurationSection keepassSection = config.GetSection("KeePassSettings");
                string? token = TokenVault.GetDiscordToken(keepassSection);

                if (string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException("Failed to obtain a token from database.");
                }

                BotConfig botConfig = new()
                {
                    Token = token,
                    TestGuildId = config.GetValue<ulong>("Discord:TestGuildId")
                };

                services.AddSingleton(botConfig);

                services.AddSingleton<DiscordSocketClient>(provider =>
                {
                    DiscordSocketConfig socketConfig = new()
                    {
                        GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers,
                        LogLevel = LogSeverity.Info,
                        AlwaysDownloadUsers = false,
                    };
                    return new DiscordSocketClient(socketConfig);
                });

                // Slash commands
                services.AddSingleton<InteractionService>(provider =>
                {
                    InteractionServiceConfig interactionConfig = new()
                    {
                        LogLevel = LogSeverity.Info,
                        DefaultRunMode = RunMode.Async,
                    };
                    return new InteractionService(
                        provider.GetRequiredService<DiscordSocketClient>(),
                        interactionConfig);
                });

                services.AddHostedService<Bot>();
                services.AddTransient<ICsvExportService, CsvExportService>();
            })
            .Build();

            await host.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
