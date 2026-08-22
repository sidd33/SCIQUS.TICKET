using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using Microsoft.IdentityModel.Tokens;
using SCIQUSTICKETS.BUSINESS.Implementations.Service;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using SCIQUSTICKETS.DATA.Implementations.Repositories;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;
using SCIQUSTICKETS.BUSINESS.Validations.Authorization;


using System.Text;



var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseMySql(
		connectionString,
		ServerVersion.AutoDetect(connectionString)
	));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, UserRole>(options =>
{
	options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtSettings["Issuer"],
		ValidAudience = jwtSettings["Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
	};
});

// Add CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:5174")
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});

builder.Services.AddOpenApi();
// Add Controllers
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = Microsoft.OpenApi.SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = Microsoft.OpenApi.ParameterLocation.Header,
		Description = "Paste your JWT here (no need to type 'Bearer' first)"
	});

	options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
	{
		[new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
	});
});
// Register CRM / Accounts Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountContactRepository, AccountContactRepository>();
builder.Services.AddScoped<IAccountAddressRepository, AccountAddressRepository>();

// Register CRM / Accounts Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountContactService, AccountContactService>();
builder.Services.AddScoped<IAccountAddressService, AccountAddressService>();

// Register Auth Repositories & Services
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register Employee Repositories & Services
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Register Department Repositories & Services
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

// Register Grade Repositories & Services
builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IGradeService, GradeService>();

builder.Services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
builder.Services.AddScoped<ITicketSubTypeRepository, TicketSubTypeRepository>();
builder.Services.AddScoped<ITicketPriorityRepository, TicketPriorityRepository>();
builder.Services.AddScoped<ITicketBusinessImpactRepository, TicketBusinessImpactRepository>();

builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
builder.Services.AddScoped<ITicketSubTypeService, TicketSubTypeService>();
builder.Services.AddScoped<ITicketPriorityService, TicketPriorityService>();
builder.Services.AddScoped<ITicketBusinessImpactService, TicketBusinessImpactService>();

builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IAssignmentEngine, AssignmentEngine>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketTimelineService, TicketTimelineService>();
builder.Services.AddScoped<ISupportPlanService, SupportPlanService>();

// Register Module 5, 6, 8 Services
builder.Services.AddScoped<ITicketNotificationService, TicketNotificationService>();
builder.Services.AddScoped<IEmailChannelService, EmailChannelService>();
builder.Services.AddScoped<IWhatsAppChannelService, WhatsAppChannelService>();

// Register Background Services
builder.Services.AddHostedService<SCIQUSTICKETS.WebAPI.BackgroundServices.EmailPollingBackgroundService>();
builder.Services.AddHostedService<SCIQUSTICKETS.WebAPI.BackgroundServices.SupportPlanExpiryJob>();

builder.Services.AddScoped<ISlaService, SlaService>();
builder.Services.AddHostedService<SCIQUSTICKETS.WebAPI.BackgroundServices.SlaBackgroundService>();

// Register custom authorization policies (SameUserOrAdmin, AdminOnly)
builder.Services.AddAuthorizationPolicies();
builder.Services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddScoped<ITicketReportService, TicketReportService>();

builder.Services.AddScoped<IAcceptanceService, AcceptanceService>();
builder.Services.AddScoped<IFaqArticleService, FaqArticleService>();
builder.Services.AddScoped<IPortalTicketService, PortalTicketService>();
builder.Services.AddScoped<ISystemActorService, SystemActorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SCIQUSTICKETS.DATA.Contexts.AppDbContext>();
    context.Database.EnsureCreated();
    await SCIQUSTICKETS.WebAPI.DbSeeder.SeedAsync(app.Services);
}

app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

