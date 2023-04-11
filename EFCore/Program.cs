using Auth.Database.DbContextConfiguration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Logging.AddSerilog().AddConsole();

builder.Services.AddMasterDbContext(builder.Configuration);

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.Run();
