using DiscordBot.Application.Interface;

namespace DiscordBot.Application.Implementation;

public class MariaDbBackupImplementation : IMariaDbBackupImplementation
{
    private readonly IMariaDbBackupRepository _repository;

    public MariaDbBackupImplementation(IMariaDbBackupRepository repository)
    {
        _repository = repository;
    }

    public async Task Backup()
    {
        await _repository.Backup();
    }
}