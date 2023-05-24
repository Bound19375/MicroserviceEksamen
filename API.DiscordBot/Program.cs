using Auth.Database;
using Auth.Database.DbContextConfiguration;
using BrokersService.MassTransitServiceCollection;
using Crosscutting.Configuration.AuthPolicyConfiguration;
using Crosscutting.Configuration.JwtConfiguration;
using Crosscutting.TransactionHandling;
using DiscordBot.Application.Implementation;
using DiscordBot.Application.Interface;
using DiscordBot.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//ILogger
builder.Logging.ClearProviders().AddSerilog().AddConsole();

//Docker
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddMasterDbContext(builder.Configuration);

//Dependency Injection
builder.Services.Scan(a => a.FromCallingAssembly().AddClasses().AsMatchingInterface().WithScopedLifetime());
//builder.Services.AddScoped<IUnitOfWork<IdentityDb>, UnitOfWork<IdentityDb>>();
builder.Services.AddScoped<IUnitOfWork<AuthDbContext>, UnitOfWork<AuthDbContext>>();
builder.Services.AddScoped<IDiscordBotQueryRepository, DiscordBotQueryRepository>();
builder.Services.AddScoped<IDiscordBotQueryImplementation, DiscordBotQueryImplementation>();
builder.Services.AddScoped<IDiscordBotCommandRepository, DiscordBotCommandRepository>();
builder.Services.AddScoped<IDiscordBotCommandImplementation, DiscordBotCommandImplementation>();
builder.Services.AddScoped<IDiscordGatewayBuyHandlerRepository, DiscordGatewayBuyHandlerRepository>();
builder.Services.AddScoped<IDiscordGatewayBuyHandlerImplementation, DiscordGatewayBuyHandlerImplementation>();
builder.Services.AddScoped<IDiscordBotCleanupRepository, DiscordBotCleanupRepository>();
builder.Services.AddScoped<IDiscordBotCleanupImplementation, DiscordBotCleanupImplementation>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//JWT
builder.Services.AddJwtConfiguration(builder.Configuration);

//Policies
builder.Services.AddClaimPolicyConfiguration();

//Kafka
builder.Services.AddMassTransitWithRabbitMqAndKafka(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();