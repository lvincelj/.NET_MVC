using System;
using System.Diagnostics;
using HospitalManagementApp.Data;
using HospitalManagementApp.Infrastructure.Logging;
using HospitalManagementApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);

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

await IdentitySeed.SeedRolesAndAdminAsync(app.Services);
await AppDataSeed.SeedDemoDataAsync(app.Services);

// Standard ASP.NET Core Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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
