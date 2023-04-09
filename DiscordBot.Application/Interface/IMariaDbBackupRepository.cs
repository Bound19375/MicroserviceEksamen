namespace DiscordBot.Application.Interface;

public interface IMariaDbBackupRepository
{
    Task Backup();
}