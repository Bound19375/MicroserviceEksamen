using Admin.Application.Implementation;
using Admin.Application.Interface;
using Admin.Infrastructure;
using Auth.Database;
using Auth.Database.DbContextConfiguration;
using Crosscutting.Configuration.AuthPolicyConfiguration;
using Crosscutting.Configuration.JwtConfiguration;
using Crosscutting.TransactionHandling;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//ILogger
builder.Logging.ClearProviders().AddSerilog().AddConsole();

//Docker
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddMasterDbContext(builder.Configuration);

//Dependency Injection
builder.Services.Scan(a => a.FromCallingAssembly().AddClasses().AsMatchingInterface().WithScopedLifetime());
//builder.Services.AddScoped<IUnitOfWork<IdentityDb>, UnitOfWork<IdentityDb>>();
builder.Services.AddScoped<IUnitOfWork<AuthDbContext>, UnitOfWork<AuthDbContext>>();
builder.Services.AddScoped<IAdminExtendLicensesRepository, AdminExtendLicensesRepository>();
builder.Services.AddScoped<IAdminExtendLicensesImplementation, AdminExtendLicensesImplementation>();


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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
