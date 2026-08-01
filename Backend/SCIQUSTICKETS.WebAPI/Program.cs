using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.Implementations.Service;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.Implementations.Repositories;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

// Add Controllers
builder.Services.AddControllers();

// Register CRM / Accounts Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountContactRepository, AccountContactRepository>();
builder.Services.AddScoped<IAccountAddressRepository, AccountAddressRepository>();

// Register CRM / Accounts Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountContactService, AccountContactService>();
builder.Services.AddScoped<IAccountAddressService, AccountAddressService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
