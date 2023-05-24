using Auth.Application.Implementation;
using Auth.Application.Interface;
using Auth.Database;
using Auth.Database.DbContextConfiguration;
using Auth.Infrastructure;
using Crosscutting.Configuration.AuthPolicyConfiguration;
using Crosscutting.Configuration.JwtConfiguration;
using Crosscutting.TransactionHandling;

var builder = WebApplication.CreateBuilder(args);

//ILogger
builder.Logging.ClearProviders().AddConsole();

//Docker
builder.Configuration.AddEnvironmentVariables();

//DbContext
builder.Services.AddMasterDbContext(builder.Configuration);

//Dependency Injection
//builder.Services.AddScoped<IUnitOfWork<IdentityDb>, UnitOfWork<IdentityDb>>();
builder.Services.AddScoped<IUnitOfWork<AuthDbContext>, UnitOfWork<AuthDbContext>>();
builder.Services.Scan(a => a.FromCallingAssembly().AddClasses().AsMatchingInterface().WithScopedLifetime());
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthImplementation, AuthImplementation>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//JWT
builder.Services.AddJwtConfiguration(builder.Configuration);

//Policies
builder.Services.AddClaimPolicyConfiguration();

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
