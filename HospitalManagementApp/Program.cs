using System;
using System.Diagnostics;
using HospitalManagementApp.Data;
using HospitalManagementApp.Infrastructure.Logging;
using HospitalManagementApp.Models;
using HospitalManagementApp.Services.Ai;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
    AddLocalUserSecretsFile(builder.Configuration);
}

builder.Logging.AddFileLogger(
    builder.Configuration.GetSection(FileLoggingOptions.SectionName),
    builder.Environment.ContentRootPath);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Navigation properties are non-nullable in models but are not posted from forms.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

var defaultConnectionString = ResolveSqliteConnectionString(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    builder.Environment);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(defaultConnectionString));

builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);
builder.Services.Configure<AiOptions>(builder.Configuration.GetSection(AiOptions.SectionName));
builder.Services.AddChatClient(services =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var apiKey = configuration["AI:OpenAI:ApiKey"];
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        apiKey = configuration["OPENAI_API_KEY"];
    }
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        apiKey = configuration["OpenAI:ApiKey"];
    }

    var model = configuration["AI:OpenAI:Model"];
    if (string.IsNullOrWhiteSpace(model))
    {
        model = "gpt-4o-mini";
    }

    if (string.IsNullOrWhiteSpace(apiKey))
    {
        return new MissingConfigurationChatClient();
    }

    return new OpenAI.Chat.ChatClient(model, apiKey).AsIChatClient();
})
.UseFunctionInvocation();
builder.Services.AddScoped<IDataAssistantToolProvider, DataAssistantToolProvider>();
builder.Services.AddScoped<IDataAssistantService, DataAssistantService>();

builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];

var hasGoogle = !string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret);
var hasFacebook = !string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret);

if (hasGoogle || hasFacebook)
{
    var authenticationBuilder = builder.Services.AddAuthentication();

    if (hasGoogle)
    {
        authenticationBuilder.AddGoogle(options =>
        {
            options.ClientId = googleClientId!;
            options.ClientSecret = googleClientSecret!;
        });
    }

    if (hasFacebook)
    {
        authenticationBuilder.AddFacebook(options =>
        {
            options.AppId = facebookAppId!;
            options.AppSecret = facebookAppSecret!;
        });
    }
}

builder.Services.AddScoped<AppointmentRepository>();
builder.Services.AddScoped<DepartmentRepository>();
builder.Services.AddScoped<DoctorRepository>();
builder.Services.AddScoped<MedicalRecordRepository>();
builder.Services.AddScoped<MedicationRepository>();
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<PrescriptionRepository>();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await ApplyDatabaseMigrationsAsync(app.Services);
}
await IdentitySeed.SeedRolesAndAdminAsync(app.Services);
await AppDataSeed.SeedDemoDataAsync(app.Services);

// Standard ASP.NET Core Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);
app.UseHttpsRedirection();
app.UseRouting();
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();

    try
    {
        await next();
    }
    finally
    {
        stopwatch.Stop();
        app.Logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
            context.Request.Method,
            context.Request.Path.Value,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
});
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string ResolveSqliteConnectionString(string? configuredConnectionString, IWebHostEnvironment environment)
{
    var connectionString = string.IsNullOrWhiteSpace(configuredConnectionString)
        ? "Data Source=HospitalManagementApp.local.db"
        : configuredConnectionString;

    var builder = new SqliteConnectionStringBuilder(connectionString);
    var isAzureAppService = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));

    if (isAzureAppService && !Path.IsPathRooted(builder.DataSource))
    {
        var dataDirectory = "/home/data";
        builder.DataSource = Path.Combine(dataDirectory, Path.GetFileName(builder.DataSource));
    }

    var databaseDirectory = Path.GetDirectoryName(builder.DataSource);
    if (!string.IsNullOrWhiteSpace(databaseDirectory))
    {
        Directory.CreateDirectory(databaseDirectory);
    }

    return builder.ToString();
}

static async Task ApplyDatabaseMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

static void AddLocalUserSecretsFile(IConfigurationBuilder configuration)
{
    const string userSecretsId = "adbe883b-735d-479e-955a-978f77236cb8";
    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    if (!string.IsNullOrWhiteSpace(home))
    {
        configuration.AddJsonFile(
            Path.Combine(home, ".microsoft", "usersecrets", userSecretsId, "secrets.json"),
            optional: true,
            reloadOnChange: true);
    }

    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    if (!string.IsNullOrWhiteSpace(appData))
    {
        configuration.AddJsonFile(
            Path.Combine(appData, "Microsoft", "UserSecrets", userSecretsId, "secrets.json"),
            optional: true,
            reloadOnChange: true);
    }
}
