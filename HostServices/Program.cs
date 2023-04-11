using Auth.Database;
using Auth.Database.DbContextConfiguration;
using Crosscutting.TransactionHandling;
using DiscordBot.Application.Implementation;
using DiscordBot.Application.Interface;
using DiscordBot.Infrastructure;
using HostServices.HostService;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders().AddSerilog().AddConsole();

builder.Services.AddMasterDbContext(builder.Configuration);

//Dependency Injection
builder.Services.Scan(a => a.FromCallingAssembly().AddClasses().AsMatchingInterface().WithScopedLifetime());
//builder.Services.AddScoped<IUnitOfWork<IdentityDb>, UnitOfWork<IdentityDb>>();
builder.Services.AddScoped<IUnitOfWork<AuthDbContext>, UnitOfWork<AuthDbContext>>();
builder.Services.AddScoped<IDiscordBotCleanupRepository, DiscordBotCleanupRepository>();
builder.Services.AddScoped<IDiscordBotCleanupImplementation, DiscordBotCleanupImplementation>();
builder.Services.AddScoped<IMariaDbBackupImplementation, MariaDbBackupImplementation>();
builder.Services.AddScoped<IMariaDbBackupRepository, MariaDbBackupRepository>();

builder.Services.AddQuartz(q =>
{
    var jobPurge = new JobKey("Purge");
    var jobBackup = new JobKey("Backup");

    q.AddJob<PurgeService>(opts =>
    {
        opts.WithIdentity(jobPurge);
    });

    q.AddJob<MariaDbBackup>(opts =>
    {
        opts.WithIdentity(jobBackup);
    });

    q.AddTrigger(opts =>
    {
        opts.ForJob(jobPurge);
        opts.WithIdentity("JobPurge-Trigger");
        opts.WithCronSchedule("0 0 0 ? * * *");
        //opts.WithSimpleSchedule(x=> {
        //    x.WithIntervalInSeconds(1);
        //    x.WithRepeatCount(0);
        //});
    });

    q.AddTrigger(opts =>
    {
        opts.ForJob(jobBackup);
        opts.WithIdentity("JobBackup-Trigger");
        opts.WithCronSchedule("0 0 0/6 ? * * *");
        //opts.WithSimpleSchedule(x =>
        //{
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
