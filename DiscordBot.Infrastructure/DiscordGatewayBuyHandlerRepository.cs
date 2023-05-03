﻿using Auth.Database;
using Auth.Database.Model;
using Crosscutting;
using Crosscutting.SellixPayload;
using DiscordBot.Application.Interface;
using DiscordSaga.Components.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DiscordBot.Infrastructure
{
    public class DiscordGatewayBuyHandlerRepository : IDiscordGatewayBuyHandlerRepository
    {
        private readonly AuthDbContext _db;
        private readonly ILogger<DiscordGatewayBuyHandlerRepository> _logger;

        public DiscordGatewayBuyHandlerRepository(AuthDbContext db, ILogger<DiscordGatewayBuyHandlerRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        async Task<LicenseNotificationEvent> IDiscordGatewayBuyHandlerRepository.OrderHandler(SellixPayloadNormal.Root root)
        {
            try
            {
                if (root.Event is "order:paid" or "order:paid:product" &&
                    root.Data.StatusHistory[0].InvoiceId != "dummy")
                {
                    var userExists = await _db.User!.FirstOrDefaultAsync(x =>
                        x.DiscordId == root.Data.CustomFields.DiscordId);

                    _logger.LogInformation(root.Data.CustomFields.DiscordId + " " + root.Data.CustomFields.DiscordUser +
                                           " @ Engaged purchase at: " + DateTime.UtcNow);

                    DateTime time = DateTime.UtcNow;
                    WhichSpec whichSpec = WhichSpec.none;
                    int quantity = 0;

                    if (root.Data.ProductTitle.Contains("AIO"))
                    {
                        whichSpec = WhichSpec.AIO;
                        quantity = Convert.ToInt32(root.Data.Quantity);
                        var currentLicenseTime = _db.ActiveLicenses.Include(user => user.User)
                            .Include(order => order.Order)
                            .Where(x => (x.User.DiscordId == root.Data.CustomFields.DiscordId ||
                                         x.User.DiscordUsername == root.Data.CustomFields.DiscordUser) &&
                                        x.ProductNameEnum == WhichSpec.AIO)
                            .MaxBy(x=>x.EndDate) ?? null;

                        if (currentLicenseTime?.EndDate != null)
                        {
                            if (currentLicenseTime.EndDate >= DateTime.UtcNow)
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

                        var currentMonthlyLicenseTime = _db.ActiveLicenses.Include(user => user.User)
                            .Where(x => (x.User.DiscordId == root.Data.CustomFields.DiscordId ||
                                         x.User.DiscordUsername == root.Data.CustomFields.DiscordUser) &&
                                        x.ProductNameEnum == whichSpec)
                            .MaxBy(x=>x.EndDate) ?? null;

                        if (currentMonthlyLicenseTime != null)
                        {
                            if (currentMonthlyLicenseTime.EndDate >= DateTime.UtcNow)
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

                        if (userExists.DiscordUsername != root.Data.CustomFields.DiscordUser)
                        {
                            userExists.DiscordUsername = root.Data.CustomFields.DiscordUser;
                            await _db.SaveChangesAsync();
                        }
                    }

                    var order = new OrderDbModel
                    {
                        UserId = dbUserId,
                        UniqId = root.Data.Uniqid,
                        ProductName = root.Data.ProductTitle,
                        ProductPrice = root.Data.TotalDisplay.ToString() ?? "null",
                        PurchaseDate = DateTime.UtcNow
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

                    var serializePayload = JsonConvert.SerializeObject(root);
                    
                    var message = new LicenseNotificationEvent
                    {
                        Payload = serializePayload,
                        Quantity = quantity,
                        Time = time.AddDays(Convert.ToInt32(1 * quantity)),
                        WhichSpec = whichSpec,
                    };

                    return message;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            throw new Exception("Unknown Error In GrantLicense");
        }
    }
}
