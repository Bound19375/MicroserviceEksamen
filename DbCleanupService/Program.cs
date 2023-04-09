using Auth.Database;
using Auth.Database.DbContextConfiguration;
using Crosscutting.TransactionHandling;
using DbCleanupService.HostService;
using DiscordBot.Application.Implementation;
using DiscordBot.Application.Interface;
using DiscordBot.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddSerilog().AddConsole();

builder.Services.AddMasterDbContext(builder.Configuration);

//Dependency Injection
builder.Services.Scan(a => a.FromCallingAssembly().AddClasses().AsMatchingInterface().WithScopedLifetime());
//builder.Services.AddScoped<IUnitOfWork<IdentityDb>, UnitOfWork<IdentityDb>>();
builder.Services.AddScoped<IUnitOfWork<AuthDbContext>, UnitOfWork<AuthDbContext>>();
builder.Services.AddScoped<IDiscordBotCleanupRepository, DiscordBotCleanupRepository>();
builder.Services.AddScoped<IDiscordBotCleanupImplementation, DiscordBotCleanupImplementation>();

//Quartz
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("Purge");
    q.AddJob<HostService>(opts =>
    {
        opts.WithIdentity(jobKey);
    });

    q.AddTrigger(opts =>
    {
        opts.ForJob(jobKey);
        opts.WithIdentity("MyJob-Trigger");
        opts.WithCronSchedule("0 0 0 ? * * *");
        //opts.WithSimpleSchedule(x=> {
        //    x.WithIntervalInSeconds(1);
        //    x.WithRepeatCount(0);
        //});
    });

    q.UseMicrosoftDependencyInjectionJobFactory();
});

builder.Services.AddQuartzHostedService(opt =>
{
    opt.WaitForJobsToComplete = true;
});

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.Run();
