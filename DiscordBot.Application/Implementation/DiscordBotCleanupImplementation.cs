using Auth.Database;
using Crosscutting.TransactionHandling;
using DiscordBot.Application.Interface;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Application.Implementation;

public class DiscordBotCleanupImplementation : IDiscordBotCleanupImplementation
{
    private readonly IDiscordBotCleanupRepository _repository;
    private readonly IUnitOfWork<AuthDbContext> _unitOfWork;
    private readonly ILogger<DiscordBotCleanupImplementation> _logger;
    public DiscordBotCleanupImplementation(IDiscordBotCleanupRepository repository, IUnitOfWork<AuthDbContext> unitOfWork, ILogger<DiscordBotCleanupImplementation> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task CleanUp()
    {
        try
        {
            await _unitOfWork.CreateTransaction(System.Data.IsolationLevel.Serializable);
            await _repository.CleanUp();
            await _unitOfWork.Commit();
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.Message);
            await _unitOfWork.Rollback();
        }
    }
}