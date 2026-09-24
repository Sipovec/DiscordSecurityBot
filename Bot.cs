using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DiscordSecurityBot.Models;

namespace DiscordSecurityBot;

public class Bot
(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    ILogger<Bot> logger,
    BotConfig config
) : BackgroundService
{
    private readonly DiscordSocketClient _client = client;
    private readonly InteractionService _interactions = interactions;
    private readonly IServiceProvider _services = services;
    private readonly ILogger<Bot> _logger = logger;
    private readonly BotConfig _config = config;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Подписка на события
        _client.Log += LogAsync;
        _client.Ready += OnReadyAsync;
        _client.InteractionCreated += OnInteractionCreatedAsync;
        _interactions.SlashCommandExecuted += OnSlashCommandExecutedAsync;

        // Добавление команд
        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        // Вход с токеном
        await _client.LoginAsync(TokenType.Bot, _config.Token);
        await _client.StartAsync();

        // Ожидание завершения работы
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Ручная остановка по Ctrl+C
        }
    }

    private Task LogAsync(LogMessage msg)
    {
        LogLevel level = LogLevelFromSeverity(msg.Severity);
        if (_logger.IsEnabled(level))
        {
            _logger.Log(level, msg.Exception, "{Message}", msg.Message);
        }
        return Task.CompletedTask;
    }

    private static LogLevel LogLevelFromSeverity(LogSeverity severity) =>
        severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information
        };

    private int _commandsRegistered = 0;

    // Регистрация команд для гильдии (ID указывается в appsettings)
    private async Task OnReadyAsync()
    {
        if (Interlocked.Exchange(ref _commandsRegistered, 1) == 0)
        {
            #if DEBUG
                await _interactions.RegisterCommandsToGuildAsync(_config.TestGuildId, deleteMissing: true);
            #else
                // Глобал. регистрация, есть задержка от минут до часа.
                // Ограничение на 100 глобал. команд, но можно делать подкоманды
                await _interactions.RegisterCommandsGloballyAsync(deleteMissing: true);
            #endif
        }
    }

    // Выполнение команды
    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        SocketInteractionContext context = new(_client, interaction);

        // Сообщение «Бот думает», пока выполняется команда
        await interaction.DeferAsync(ephemeral: true);

        // Выполнение команды
        IResult result = await _interactions.ExecuteCommandAsync(context, _services);

        if (!result.IsSuccess)
        {
            // Если команда вообще не нашлась или произошла ошибка до её запуска
            _logger.LogError("Ошибка запуска: {ErrorReason}", result.ErrorReason);
            await interaction.FollowupAsync($"Не удалось запустить команду: {result.ErrorReason}", ephemeral: true);
        }
    }

    // Обработчик завершения команд
    private async Task OnSlashCommandExecutedAsync(SlashCommandInfo info, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess)
        {
            return;
        }

        _logger.LogError("Команда {CommandName} завершилась с ошибкой: {ErrorReason}", info.Name, result.ErrorReason);

        switch (result.Error)
        {
            // [RequireBotPermission]
            case InteractionCommandError.UnmetPrecondition:
                await context.Interaction.FollowupAsync($"Недостаточно прав: {result.ErrorReason}", ephemeral: true);
                break;

            // Исключение
            case InteractionCommandError.Exception:
                await context.Interaction.FollowupAsync("Критическая ошибка.", ephemeral: true);
                break;

            default:
                await context.Interaction.FollowupAsync($"Ошибка: {result.ErrorReason}", ephemeral: true);
                break;
        }
    }

    // При завершении работы
    public override async Task StopAsync(CancellationToken stopToken)
    {
        // Отписка от событий (в обратном порядке)
        _interactions.SlashCommandExecuted -= OnSlashCommandExecutedAsync;
        _client.InteractionCreated -= OnInteractionCreatedAsync;
        _client.Ready -= OnReadyAsync;
        _client.Log -= LogAsync;

        // Завершение соединения с Discord
        await _client.LogoutAsync();
        await _client.StopAsync();

        // Остановка BackgroundService
        await base.StopAsync(stopToken);
    }
}
