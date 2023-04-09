using System.Diagnostics;
using System.Text;
using Auth.Database;
using Crosscutting.DiscordConnectionHandler.DiscordClientLibrary;
using Discord;
using Discord.WebSocket;
using DiscordBot.Application.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;


namespace DiscordBot.Infrastructure;

public class MariaDbBackupRepository : IMariaDbBackupRepository
{
    private readonly AuthDbContext _authDbContext;
    private readonly IConfiguration _configuration;
    private readonly DiscordSocketClient _client = DiscordClient.GetDiscordSocketClient();
    private readonly ILogger<MariaDbBackupRepository> _logger;

    public MariaDbBackupRepository(AuthDbContext authDbContext, IConfiguration configuration, ILogger<MariaDbBackupRepository> logger)
    {
        _authDbContext = authDbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Backup()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("BoundcoreMaster");

            await using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand();
            cmd.Connection = conn;
            using var backup = new MySqlBackup(cmd);

            var fileName = $"MariaDbBackup_{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}.sql";

            var backupString = backup.ExportToString();

            var backupStream = new MemoryStream(Encoding.UTF8.GetBytes(backupString));

            var privateEmbed = new EmbedBuilder()
                .WithThumbnailUrl("https://i.imgur.com/dxCVy9r.png")
                .AddField($"backup_{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}", "")
                .WithColor(Color.DarkOrange)
                .WithCurrentTimestamp()
                .Build();

            var privateChannel = await _client.GetChannelAsync(1094738809121423380); //backupChannel
            var textNotifier = privateChannel as SocketTextChannel;
            await textNotifier!.SendMessageAsync(embed: privateEmbed);
            await textNotifier!.SendFileAsync(backupStream, fileName, "", false, null);

            await conn.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup Service Error");
        }
    }
}