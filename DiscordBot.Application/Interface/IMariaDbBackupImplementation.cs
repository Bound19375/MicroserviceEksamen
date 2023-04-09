namespace DiscordBot.Application.Interface;

public interface IMariaDbBackupImplementation
{
    Task Backup();
}