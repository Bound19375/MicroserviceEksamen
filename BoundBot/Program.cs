using BoundBot.Services;
using Crosscutting.DiscordConnectionHandler.DiscordClientLibrary;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace BoundBot;

public class DiscordBot
{
    static readonly DiscordSocketClient Client = DiscordClient.GetDiscordSocketClient();
    static readonly CommandService Service = DiscordClient.GetCommandService();

    public static Task Main(string[] args) => new DiscordBot().MainAsync();

    async Task MainAsync()
    {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var service = new ServiceCollection()
            .AddScoped(sp => new HttpClient {
                BaseAddress = new Uri(configBuilder["HttpClient:connStr"] ?? string.Empty)
            });
        service.BuildServiceProvider();

        Client.Log += Log;

        CommandHandler cHandler = new(Client, Service);
        await cHandler.InstallCommandsAsync();
        
        SlashCommandsBuilder builder = new(Client);
        Client.Ready += builder.Client_Ready;
        SlashCommandsHandler sHandler = new(Client, configBuilder);
        Client.SlashCommandExecuted += sHandler.SlashCommandHandler;

        // Block this task until the program is closed.
        await Task.Delay(Timeout.Infinite);
    }

    public static DiscordSocketClient GetClient()
    {
        return Client;
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}