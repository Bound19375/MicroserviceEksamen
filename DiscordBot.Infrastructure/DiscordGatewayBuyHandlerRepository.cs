using System.Configuration;
using Auth.Database;
using Auth.Database.Model;
using Crosscutting;
using Discord;
using DiscordBot.Application.Interface;
using Microsoft.Extensions.Logging;
using DiscordBot;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Crosscutting.DiscordConnectionHandler.DiscordClientLibrary;
using Crosscutting.SellixPayload;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Infrastructure
{
    public class DiscordGatewayBuyHandlerRepository : IDiscordGatewayBuyHandlerRepository
    {
        private readonly AuthDbContext _db;
        private readonly ILogger<DiscordGatewayBuyHandlerRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client = DiscordClient.GetDiscordSocketClient();

        public DiscordGatewayBuyHandlerRepository(AuthDbContext db, ILogger<DiscordGatewayBuyHandlerRepository> logger, IConfiguration configuration)
        {
            _db = db;
            _logger = logger;
            _configuration = configuration;
        }

        async Task IDiscordGatewayBuyHandlerRepository.OrderHandler(SellixPayloadNormal.Root root)
        {
            try
            {
                if (root.Event is "order:paid" or "order:paid:product" && root.Data.StatusHistory[0].InvoiceId != "dummy")
                {
                    var userExists = await _db.User!.FirstOrDefaultAsync(x => x.DiscordId == root.Data.CustomFields.DiscordId && x.DiscordUsername == root.Data.CustomFields.DiscordUser);

                    _logger.LogInformation(root.Data.CustomFields.DiscordId + " " + root.Data.CustomFields.DiscordUser + " @ Engaged purchase at: " + DateTime.Now);

                    DateTime time = DateTime.Now;
                    WhichSpec whichSpec = WhichSpec.none;
                    int? quantity = 0;

                    if (root.Data.ProductTitle.Contains("AIO"))
                    {
                        whichSpec = WhichSpec.AIO;
                        quantity = Convert.ToInt32(root.Data.Quantity);
                        var currentLicenseTime = await _db.ActiveLicenses.Include(user => user.User).Include(order=>order.Order)
                            .Where(x => (x.User.DiscordId == root.Data.CustomFields.DiscordId ||
                                         x.User.DiscordUsername == root.Data.CustomFields.DiscordUser) &&
                                        x.ProductNameEnum == WhichSpec.AIO)
                            .FirstOrDefaultAsync();
                        
                        if (currentLicenseTime?.EndDate != null)
                        {
                            if (currentLicenseTime.EndDate >= DateTime.Now)
                            {
                                time = currentLicenseTime.EndDate;
                            }
                            else
                            {
                                _db.ActiveLicenses.Remove(currentLicenseTime);
                                await _db.SaveChangesAsync();
                            }
                        }
                    }
                    else
                    {
                        whichSpec = root.Data.ProductTitle switch
                        {
                            "test" => WhichSpec.test,
                            _ => 0
                        };

                        quantity = 30 * Convert.ToInt32(root.Data.Quantity);

                        var currentMonthlyLicenseTime = await _db.ActiveLicenses.Include(user => user.User)
                            .Where(x => (x.User.DiscordId == root.Data.CustomFields.DiscordId ||
                                         x.User.DiscordUsername == root.Data.CustomFields.DiscordUser) &&
                                        x.ProductNameEnum == whichSpec)
                            .FirstOrDefaultAsync();

                        if (currentMonthlyLicenseTime != null)
                        {
                            if (currentMonthlyLicenseTime.EndDate >= DateTime.Now)
                            {
                                time = currentMonthlyLicenseTime.EndDate;
                            }
                            else
                            {
                                _db.ActiveLicenses.Remove(currentMonthlyLicenseTime);
                                await _db.SaveChangesAsync();
                            }
                        }
                    }

                    string dbUserId;

                    if (userExists == null)
                    {
                        var user = new UserDbModel
                        {
                            Email = root.Data.CustomerEmail,
                            Firstname = root.Data.CustomFields.Name ?? "CryptoBuyer",
                            Lastname = root.Data.CustomFields.Surname ?? "CryptoBuyer",
                            DiscordUsername = root.Data.CustomFields.DiscordUser,
                            DiscordId = root.Data.CustomFields.DiscordId,
                            HWID = root.Data.CustomFields.HWID
                        };

                        await _db.User.AddAsync(user);
                        await _db.SaveChangesAsync();

                        dbUserId = user.UserId;
                    }
                    else 
                    {
                        dbUserId = userExists.UserId;
                    }

                    var order = new OrderDbModel
                    {
                        UserId = dbUserId,
                        UniqId = root.Data.Uniqid,
                        ProductName = root.Data.ProductTitle,
                        ProductPrice = root.Data.TotalDisplay.ToString(),
                        PurchaseDate = DateTime.Now,
                    };

                    await _db.Order!.AddAsync(order);
                    await _db.SaveChangesAsync();

                    var licenses = new ActiveLicensesDbModel
                    {
                        UserId = dbUserId,
                        ProductName = root.Data.ProductTitle,
                        ProductNameEnum = whichSpec,
                        EndDate = time.AddDays(Convert.ToInt32(1 * quantity)),
                        OrderId = order.OrderId
                    };

                    await _db.ActiveLicenses!.AddAsync(licenses);
                    await _db.SaveChangesAsync();

                    var clientUser = await _client.GetUserAsync(ulong.Parse(root.Data.CustomFields.DiscordId));

                    if (clientUser != null)
                    {
                        var guild = _client.GetGuild(ulong.Parse(_configuration["Discord:Guid"]));
                        IGuildUser? guildUser = null;
                        if (guild != null)
                        {
                            guildUser = guild.GetUser(clientUser.Id);
                        }

                        if (guildUser != null && guild != null)
                        {
                            ulong roleId = (ulong)(whichSpec == WhichSpec.AIO ? 986361482377826334 : 911959454323445840);
                            var role = guild.GetRole(roleId);
                            await guildUser.AddRoleAsync(role);
                        }

                        bool couldSendToUser = false;
                        try
                        {
                            var embed = new EmbedBuilder()
                            .WithThumbnailUrl("https://i.imgur.com/dxCVy9r.png")
                            .AddField("Confirmation", $"You've been successfully added to the database & roled!" +
                                $"\nOrderId: {root.Data.Uniqid}" +
                                $"\nProduct: {root.Data.ProductTitle}" +
                                $"\nEndDate: {time.AddDays(Convert.ToInt32(1 * quantity))}" +
                                "\nPlease read the instruction channels & faq!")
                            .WithColor(Color.DarkOrange)
                            .WithCurrentTimestamp()
                            .Build();

                            await clientUser.SendMessageAsync("", false, embed);
                            couldSendToUser = true;
                        }
                        catch
                        {
                            _logger.LogInformation("Wasn't able to DM: " + root.Data.CustomFields.DiscordUser);
                        }
                        
                        var privateEmbed = new EmbedBuilder()
                            .WithThumbnailUrl("https://i.imgur.com/dxCVy9r.png")
                            .AddField("Confirmation", 
                                $"\n{clientUser.Mention} has been successfully added to the database & roled!" +
                                $"\nUser Notified: {couldSendToUser}" +
                                $"\nOrderId: {root.Data.Uniqid}" +
                                $"\nProduct: {root.Data.ProductTitle}" +
                                $"\nEndDate: {time.AddDays(Convert.ToInt32(1 * quantity))}")
                            .WithColor(Color.DarkOrange)
                            .WithCurrentTimestamp()
                            .Build();

                        var privateChannel = await _client.GetChannelAsync(862658521065848872); //NotifyChannel
                        var textNotifier = privateChannel as SocketTextChannel; 
                        await textNotifier!.SendMessageAsync("", false, privateEmbed);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
