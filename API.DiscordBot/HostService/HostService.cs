using DiscordBot.Application.Interface;
using Quartz;

namespace API.DiscordBot.HostService
{
    [DisallowConcurrentExecution]
    public class HostService : IJob
    {
        private readonly IDiscordBotCleanupImplementation _cleanUp;
        public HostService(IDiscordBotCleanupImplementation cleanup) 
        {
            _cleanUp = cleanup;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync("Executing background job");
            await _cleanUp.CleanUp();
        }
    }
}
