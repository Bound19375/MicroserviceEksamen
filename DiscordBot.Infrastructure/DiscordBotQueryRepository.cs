using Auth.Database;
using Crosscutting;
using DiscordBot.Application.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Infrastructure
{
    public class DiscordBotQueryRepository : IDiscordBotQueryRepository
    {
        private readonly ILogger _logger;
        private readonly AuthDbContext _db;

        public DiscordBotQueryRepository(AuthDbContext db, ILogger<DiscordBotQueryRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        async Task<List<AuthModelDTO>> IDiscordBotQueryRepository.CheckDB(string username, string id)
        {
            try
            {
                _logger.LogInformation(username + " Engaged CheckDB At: " + DateTime.UtcNow);

                var db = _db.ActiveLicenses.Include(User => User.User).Where(x => x.User.DiscordUsername == username || x.User.DiscordId == id).ToList();

                if (db.Count == 0) { throw new Exception("No Active Licenses For Given User"); }

                var result = new List<AuthModelDTO>();

                foreach (var item in db)
                {
                    result.Add(new AuthModelDTO
                    {
                        DiscordUsername = item.User.DiscordUsername,
                        DiscordId = item.User.DiscordId,
                        Firstname = item.User.Firstname,
                        Lastname = item.User.Lastname,
                        Email = item.User.Email,
                        ProductName = item.ProductName,
                        HWID = item.User.HWID,
                        EndDate = item.EndDate,
                    });
                }
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<AuthModelDTO>> CheckMe(string username, string id)
        {
            try
            {
                _logger.LogInformation(username + " Engaged CheckDB At: " + DateTime.UtcNow);

                var db = _db.ActiveLicenses.Include(User => User.User).Where(x => x.User.DiscordUsername == username || x.User.DiscordId == id).ToList();

                if (db.Count == 0) { throw new Exception("No Active Licenses Found!"); }

                var result = new List<AuthModelDTO>();

                foreach (var item in db)
                {
                    result.Add(new AuthModelDTO
                    {
                        DiscordUsername = item.User.DiscordUsername,
                        DiscordId = item.User.DiscordId,
                        Email = item.User.Email,
                        ProductName = item.ProductName,
                        HWID = item.User.HWID,
                        EndDate = item.EndDate,
                    });
                }
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}