using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using photoCon.Data;
using photoCon.Interface;
using photoCon.Repository;
using photoCon.Models;
using photoCon.Services;
using Serilog;
using photoCon.Dto;
using photoCon.Helper;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Read configuration from appsettings.json
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // Use Serilog for logging


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<ISeedDatabaseRepository, SeedDatabaseRepository>();
builder.Services.AddScoped<IJudgeRepository, JudgeRepository>();
builder.Services.AddScoped<ISeedControlTotalRepository, SeedControlTotalRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IParametersRepository, ParametersRepository>();
builder.Services.AddScoped<IProcessParameterRepository, ProcessParameterRepository>();
builder.Services.AddScoped<IBatchImageRepository, BatchImageRepository>();
builder.Services.AddScoped<IPhotoLocationsRepository, PhotoLocationsRepository>();
builder.Services.AddScoped<IDayNumberRepository, DayNumberRepository>();
builder.Services.AddScoped<IRankRepository, RankRepository>();
builder.Services.AddScoped<IImageScoreRepository, ImageScoreRepository>();
builder.Services.AddScoped<ICSVDetailsRepository, CSVDetailsRepository>();
builder.Services.AddScoped<IAccountManagementRepository, AccountManagementRepository>();
builder.Services.AddScoped<IAuditLogsRepository, AuditLogsRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();




builder.Services.AddDbContext<TallyProgramContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnStr"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
           //.EnableSensitiveDataLogging(); // Enable sensitive data logging here
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnStr")));



builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.AllowedForNewUsers = false;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

    options.User.RequireUniqueEmail = true;

    options.Stores.MaxLengthForKeys = 128;
    options.SignIn.RequireConfirmedEmail = true;

})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddRoles<IdentityRole>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.LoginPath = "/Home/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Strict; // Adjust as needed ----
});

builder.Services.Configure<ConfigurationAppInfo>(builder.Configuration.GetSection("AppInfo"));
builder.Services.AddScoped<RepositoryOperations>();


builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Add authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    //options.Cookie.Name = "ppc_cookie_auth";
    //options.Cookie.SameSite = SameSiteMode.None; // Adjust as needed
    // Other options as needed

    options.Cookie.Name = "ppc_cookie_auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure cookies are always sent over HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Adjust as needed
});

//// Configure logging
//builder.Logging.AddConsole(); // Add console logging

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Apply pending migrations
    dbContext.Database.Migrate();

    //// Seed the database
    //var seeder = new SeedDatabaseService(dbContext, userManager, roleManager);
    //seeder.SeedData();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); // Use session middleware

app.UseRouting();

// Use authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();
