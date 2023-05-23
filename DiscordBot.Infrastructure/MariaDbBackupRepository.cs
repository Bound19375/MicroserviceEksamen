using System.Runtime.CompilerServices;
using System.Text;
using Auth.Database;
using BoundBot.Connection.DiscordConnectionHandler.DiscordClientLibrary;
using Discord;
using Discord.WebSocket;
using DiscordBot.Application.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace DiscordBot.Infrastructure;

public class MariaDbBackupRepository : IMariaDbBackupRepository
{
    private readonly AuthDbContext _authDbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MariaDbBackupRepository> _logger;

    public MariaDbBackupRepository(AuthDbContext authDbContext, IConfiguration configuration, ILogger<MariaDbBackupRepository> logger)
    {
        _authDbContext = authDbContext;
        _configuration = configuration;
        _logger = logger;
    }

    async Task IMariaDbBackupRepository.Backup()
    {
        try
        {
            DiscordSocketClient client = DiscordClient.GetDiscordSocketClient(_configuration["Discord:Token"] ?? string.Empty);

            var connectionString = _configuration.GetConnectionString("BoundcoreMaster");

            await using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand();
            cmd.Connection = conn;

            using var backup = new MySqlBackup(cmd);

            var fileName = $"MariaDbBackup_{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}.sql";

            var backupString = backup.ExportToString();

            var backupStream = new MemoryStream(Encoding.UTF8.GetBytes(backupString));

            var guild = client.GetGuild(ulong.Parse(_configuration["Discord:Guid"]!));
            var role = guild.GetRole(1095419684892975325); //BackupRole

            var privateChannel = await client.GetChannelAsync(1094738809121423380); //backupChannel
            var textNotifier = privateChannel as IMessageChannel;
            await textNotifier!.SendFileAsync(backupStream, fileName, $"{role.Mention}\n:white_small_square:Backup_{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}", false, null);

            await conn.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup Service Error");
        }
    }
}