using System.Text;
using Auth.Database;
using Confluent.Kafka;
using Crosscutting.TransactionHandling;
using DiscordBot.Application.Implementation;
using DiscordBot.Application.Interface;
using DiscordBot.Infrastructure;
using DiscordNetConsumers;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

//ILogger
builder.Logging.ClearProviders().AddConsole();

//Docker
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
//dotnet tool update --global dotnet-ef
//dotnet ef migrations add 0.4 --project Auth.Database --startup-project BoundCoreWebApplication --context AuthDbContext
//dotnet ef database update --project Auth.Database --startup-project BoundCoreWebApplication --context AuthDbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("BoundcoreMaster") ?? throw new InvalidOperationException(),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("BoundcoreMaster")) ?? throw new InvalidOperationException(),
        x =>
        {

        });
});

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("hwid", policy => policy.RequireClaim("hwid"));
    options.AddPolicy("staff", policy => policy.RequireClaim("staff"));
    options.AddPolicy("admin", policy => policy.RequireClaim("admin"));
});

builder.Services.AddMassTransit(x =>
{
    //x.UsingInMemory();
    x.AddLogging();

    x.UsingRabbitMq((context, cfg) => {
        cfg.Host("rabbitmq", "/", h => {
            h.Username(builder.Configuration["RabbitMQ:User"]);
            h.Password(builder.Configuration["RabbitMQ:Pass"]);
        });
        cfg.ConfigureEndpoints(context);
    });

    x.AddRider(r => {
        r.AddConsumer<DiscordNotificationConsumer>();
        r.AddProducer<KafkaNotificationMessageDto>("Discord-Payment-Notification");

        r.UsingKafka((context, cfg) => {
            cfg.ClientId = "Api.Discord";

            cfg.Host("kafka");

            cfg.TopicEndpoint<KafkaNotificationMessageDto>("Discord-Payment-Notification", "Discord", e => {
                e.AutoOffsetReset = AutoOffsetReset.Earliest;
                e.ConfigureConsumer<DiscordNotificationConsumer>(context);
            });
        });
    });
});

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