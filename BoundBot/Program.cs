using BoundBot.Connection.DiscordConnectionHandler.DiscordClientLibrary;
using BoundBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace BoundBot;

public class DiscordBot
{
    public static Task Main(string[] args) => new DiscordBot().MainAsync();

    async Task MainAsync()
    {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var service = new ServiceCollection();
        service.AddSingleton<IConfiguration>(configBuilder);
        service.AddSingleton(DiscordClient.GetDiscordSocketClient(configBuilder["Discord:Token"] ?? string.Empty));
        service.AddSingleton(DiscordClient.GetCommandService());
        service.AddHttpClient("httpClient", httpClient =>
        {
            httpClient.BaseAddress = new Uri(configBuilder["HttpClient:connStr"] ?? string.Empty);
        });
        service.AddScoped<SlashCommandsHandler>();
        service.AddScoped<CommandHandler>();
        service.AddScoped<SlashCommandsBuilder>();
        var serviceProvider = service.BuildServiceProvider();

        DiscordSocketClient client = serviceProvider.GetService<DiscordSocketClient>()!;
        CommandService commandService = serviceProvider.GetRequiredService<CommandService>();

        client.Log += Log;

        CommandHandler cHandler = new(client, commandService);
        await cHandler.InstallCommandsAsync();

        var builder = serviceProvider.GetService<SlashCommandsBuilder>();
        client.Ready += builder!.Client_Ready;

        var sHandler = serviceProvider.GetService<SlashCommandsHandler>();
        client.SlashCommandExecuted += sHandler!.SlashCommandHandler;

        // Block this task until the program is closed.
        await Task.Delay(Timeout.Infinite);
    }


    //AddScoped is used when you want to create a new instance of a service for each request within the scope.This means that if you request the same service multiple times within the same scope, you'll get the same instance.
    //AddTransient is used when you want to create a new instance of a service every time it is requested.This means that if you request the same service multiple times, you'll get a different instance each time.
    //AddSingleton is used when you want to create a single instance of a service for the lifetime of the application.This means that if you request the same service multiple times, you'll get the same instance each time.

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}