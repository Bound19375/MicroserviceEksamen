using System.Runtime.InteropServices.JavaScript;
using Auth.Application.Interface;
using Auth.Database;
using Auth.Database.Model;
using Crosscutting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AuthDbContext _db;
        private readonly ILogger _logger;
        public AuthRepository(AuthDbContext db, ILogger<AuthRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        async Task<List<AuthModelDTO>> IAuthRepository.Auth(AuthModelDTO model)
        {
            try
            {
                _logger.LogInformation(model.HWID + " Engaged Auth At: " + DateTime.Now);

                var returnList = new List<AuthModelDTO>();

                var auth = await _db.ActiveLicenses.Include(user => user.User).Where(user => user.User.HWID == model.HWID && DateTime.Now < user.EndDate).ToListAsync();

                if (auth.Any())
                {
                    var authList = auth.Select(ele => new AuthModelDTO
                        {
                            Email = ele.User.Email,
                            Firstname = ele.User.Firstname,
                            Lastname = ele.User.Lastname,
                            DiscordUsername = ele.User.DiscordUsername,
                            DiscordId = ele.User.DiscordId,
                            HWID = ele.User.HWID,
                            //
                            ProductName = ele.ProductName,
                            EndDate = ele.EndDate,
                            UserId = ele.UserId,
                            ProductNameEnum = ele.ProductNameEnum
                        })
                        .ToList();

                    returnList.AddRange(authList);
                }

                var deprecatedAuth = await _db.MakeDatabase.Where(check => check.HWID == model.HWID).ToListAsync();

                if (deprecatedAuth.Any())
                {
                    var activeDeprecatedLicenses = (from ele in deprecatedAuth
                        let whichSpec = ele.Product switch
                        {
                            "Staff" => 380,
                            "AIO [1 Day]" => 1,
                            "AIO [1 Day] [CRYPTO]" => 1,
                            "AIO [180 Days]" => 180,
                            "AIO [180 Days] [CRYPTO]" => 180,
                            "AIO [30 Days]" => 30,
                            "AIO [30 Days] [CRYPTO]" => 30,
                            "AIO [365 Days]" => 365,
                            "AIO [365 Days] [CRYPTO]" => 365,
                            "AIO [60 Days]" => 60,
                            "AIO [60 Days] [CRYPTO]" => 60,
                            "AIO [90 Days]" => 90,
                            "AIO [90 Days] [CRYPTO]" => 90,
                            _ => 0
                        }
                        where DateTime.Now < ele.PurchaseDate.AddDays(whichSpec)
                        select new AuthModelDTO
                        {
                            Email = ele.Email,
                            Firstname = string.Empty,
                            Lastname = string.Empty,
                            DiscordUsername = ele.DiscordUsername,
                            DiscordId = ele.DiscordID,
                            HWID = ele.HWID,
                            //
                            ProductName = ele.Product,
                            EndDate = ele.PurchaseDate.AddDays(whichSpec),
                            UserId = ele.Key,
                            ProductNameEnum = 0,
                        }).ToList();

                    returnList.AddRange(activeDeprecatedLicenses);
                }

                if (returnList.Any())
                {
                    return returnList;
                }
                else
                {
                    throw new Exception("No Active Licenses");
                }
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message);
            }
        }
    }
}