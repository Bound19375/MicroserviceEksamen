using System.Configuration;
using BoundBot.Services;
using Crosscut.DiscordConnectionHandler.DiscordClientLibrary;
using Discord;
using Discord.Commands;
using Discord.WebSocket;


namespace BoundBot;

public class DiscordBot
{
    static DiscordSocketClient Client = DiscordClient.GetDiscordSocketClient();
    static CommandService Service = DiscordClient.GetCommandService();

    public static Task Main(string[] args) => new DiscordBot().MainAsync();

    async Task MainAsync()
    {
        Client.Log += Log;

        CommandHandler cHandler = new(Client, Service);
        await cHandler.InstallCommandsAsync();
        
        SlashCommandsBuilder builder = new(Client);
        Client.Ready += builder.Client_Ready;
        SlashCommandsHandler sHandler = new(Client);
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