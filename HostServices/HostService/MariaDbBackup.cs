using DiscordBot.Application.Interface;
using Quartz;

namespace HostServices.HostService
{
    [DisallowConcurrentExecution]
    public class MariaDbBackup : IJob
    {
        private readonly IMariaDbBackupImplementation _implementation;

        public MariaDbBackup(IMariaDbBackupImplementation implementation)
        {
            _implementation = implementation;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync("Executing background Backup job");

            await _implementation.Backup();
        }
    }
}
