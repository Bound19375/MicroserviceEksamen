using Auth.Database;
using Auth.Database.Model;
using Crosscutting;
using Crosscutting.DiscordConnectionHandler.DiscordClientLibrary;
using Discord;
using Discord.WebSocket;
using DiscordBot.Application.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DiscordBot.Infrastructure
{
    public class DiscordBotCommandRepository : IDiscordBotCommandRepository
    {
        private readonly ILogger _logger;
        private readonly AuthDbContext _db;
        private readonly IConfiguration _configuration;

        public DiscordBotCommandRepository(ILogger<DiscordBotCommandRepository> logger, AuthDbContext db, IConfiguration configuration)
        {
            _logger = logger;
            _db = db;
            _configuration = configuration;
        }

        async Task<string> IDiscordBotCommandRepository.GetStaffLicense(DiscordModelDto model)
        {
            try
            {
                _logger.LogInformation(model.DiscordUsername + " Engaged GetStaffLicense At: " + DateTime.UtcNow);

                var check = await _db.ActiveLicenses.Include(user => user.User).Include(order => order.Order).Where(x => x.User.DiscordId == model.DiscordId || x.User.DiscordUsername == model.DiscordUsername).ToListAsync();
                var userExists = await _db.User!.FirstOrDefaultAsync(x => x.DiscordId == model.DiscordId && x.DiscordUsername == model.DiscordUsername);

                if (check.Any())
                {
                    foreach (var item in check)
                    {
                        if (item.ProductName == "STAFF")
                        {
                            throw new Exception($"{model.DiscordUsername} already has attached stafflicense!");
                        }
                    }
                }

                string dbUserId;

                if (userExists == null)
                {
                    var user = new UserDbModel
                    {
                        Email = model.DiscordUsername,
                        Firstname = model.DiscordUsername,
                        Lastname = model.DiscordUsername,
                        DiscordUsername = model.DiscordUsername,
                        DiscordId = model.DiscordId,
                        HWID = model.Hwid,
                    };

                    await _db.User.AddAsync(user);
                    await _db.SaveChangesAsync();

                    dbUserId = user.UserId;
                }
                else
                {
                    dbUserId = userExists.UserId;
                }

                var Order = new OrderDbModel
                {
                    UserId = dbUserId,
                    UniqId = "STAFF",
                    ProductName = "STAFF",
                    ProductPrice = "STAFF",
                    PurchaseDate = DateTime.UtcNow,
                };

                await _db.Order.AddAsync(Order);
                await _db.SaveChangesAsync();

                var Licenses = new ActiveLicensesDbModel
                {
                    ProductName = "STAFF",
                    ProductNameEnum = WhichSpec.AIO,
                    EndDate = DateTime.UtcNow.AddDays(365),
                    UserId = dbUserId,
                    OrderId = Order.OrderId
                };

                await _db.ActiveLicenses.AddAsync(Licenses);
                await _db.SaveChangesAsync();

                return await Task.FromResult("Stafflicense Has been created:" +
                    $"\nName    : {Licenses.ProductName}" +
                    $"\nType    : {Licenses.ProductNameEnum}" +
                    $"\nEnd     : {Licenses.EndDate}" +
                    $"\nUserId  : {Licenses.UserId}" +
                    $"\nOrderId : {Licenses.OrderId}");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        async Task<string> IDiscordBotCommandRepository.UpdateDiscordAndRole(DiscordModelDto model)
        {
            try
            {
                DiscordSocketClient client = DiscordClient.GetDiscordSocketClient(_configuration["Discord:Token"] ?? string.Empty);

                _logger.LogInformation(model.DiscordUsername + " Engaged UpdateDiscord At: " + DateTime.UtcNow);

                var check = await _db.ActiveLicenses.Include(user => user.User).Where(x => x.User.DiscordId == model.DiscordId || x.User.DiscordUsername == model.DiscordUsername).ToListAsync();
                var makeCheck = await _db.MakeDatabase.Where(discord => discord.DiscordID == model.DiscordId || discord.DiscordUsername == model.DiscordUsername).ToListAsync();

                if (!check.Any() && !makeCheck.Any()) { throw new Exception($"{model.DiscordUsername} doesn't exist in the database"); }

                int activeLicenses = check.Count(item => item.EndDate < DateTime.UtcNow) + makeCheck.Count();

                if (activeLicenses >= 0)
                {
                    var guild = client.GetGuild(ulong.Parse(_configuration["Discord:Guid"]!));
                    var clientUser = await client.GetUserAsync(ulong.Parse(model.DiscordId));

                    IGuildUser? guildUser = null;
                    if (guild != null)
                    {
                        guildUser = guild.GetUser(clientUser.Id);
                    }

                    bool flag = false;
                    foreach (var item in check)
                    {
                        item.User.DiscordId = model.DiscordId;
                        item.User.DiscordUsername = model.DiscordUsername;

                        if (guildUser != null)
                        {
                            ulong roleId = (ulong)(item.ProductNameEnum == WhichSpec.AIO ? 986361482377826334 : 911959454323445840);
                            var role = guild!.GetRole(roleId);
                            await guildUser.AddRoleAsync(role);
                        }

                        flag = true;
                    }

                    if (!flag)
                    {
                        if (guildUser != null)
                        {
                            var role = guild!.GetRole(911959454323445840);
                            await guildUser.AddRoleAsync(role);
                        }
                    }
                    await _db.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("No Current Active Licenses.");
                }

                return await Task.FromResult("Successfully Updated Discord User & Roles");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        async Task<string> IDiscordBotCommandRepository.UpdateHwid(DiscordModelDto model)
        {
            try
            {
                _logger.LogInformation(model.DiscordId + " Engaged UpdateHwid At: " + DateTime.UtcNow);

                var check = await _db.ActiveLicenses.Include(user => user.User).Include(order => order.Order).Where(x => x.User.DiscordId == model.DiscordId || x.User.DiscordUsername == model.DiscordUsername).ToListAsync();

                //if (!check.Any()) { throw new Exception($"{model.DiscordUsername} doesn't exist in the database"); }

                int activeLicenses = 0;

                foreach (var item in check)
                {
                    if (item.EndDate < DateTime.UtcNow) { activeLicenses++; }
                }


                StringBuilder builder = new();

                if (activeLicenses >= 0)
                {
                    foreach (var item in check)
                    {
                        item.User.HWID = model.Hwid;

                        builder.AppendLine(item.Order.UniqId);

                    }
                    await _db.SaveChangesAsync();
                }

                //
                var Deprecatedcheck = await _db.MakeDatabase.Where(x => x.HWID == model.Hwid || x.DiscordUsername == model.DiscordUsername || x.DiscordID == model.DiscordId).ToListAsync();

                //if (!check.Any()) { throw new Exception($"{model.DiscordUsername} doesn't exist in the database"); }

                foreach (var item in Deprecatedcheck)
                {
                    item.HWID = model.Hwid;

                    builder.AppendLine(item.OrderID);

                }
                await _db.SaveChangesAsync();

                if (builder.Length == 0) { throw new Exception($"{model.DiscordUsername} doesn't exist in the database"); } //Remove This

                return await Task.FromResult("Hwid & Roles Updated License(s):" +
                    $"\n{builder}");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
