using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SCIQUSTICKETS.BUSINESS.Implementations.Service;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.BUSINESS.Validations.Authorization;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using SCIQUSTICKETS.DATA.Implementations.Repositories;
using SCIQUSTICKETS.DATA.Interfaces.IRepositories;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
	"appsettings.Local.json",
	optional: true,
	reloadOnChange: true
);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

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
		policy.WithOrigins(
			"http://localhost:5173", "https://localhost:5173",
			"http://localhost:5174", "https://localhost:5174",
			"http://localhost:3000", "https://localhost:3000",
			"http://localhost:5175", "https://localhost:5175"
		)
		.AllowAnyHeader()
		.AllowAnyMethod()
		.AllowCredentials();
	});
});

// Add Controllers
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Paste your JWT here (no need to type 'Bearer' first)"
	});

	options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new List<string>()
		}
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
	app.UseSwagger();
	app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}

using (var scope = app.Services.CreateScope())
{
    await SCIQUSTICKETS.WebAPI.DbSeeder.SeedAsync(app.Services);
}

app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();