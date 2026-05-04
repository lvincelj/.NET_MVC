using System;
using HospitalManagementApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AppointmentRepository>();
builder.Services.AddScoped<DepartmentRepository>();
builder.Services.AddScoped<DoctorRepository>();
builder.Services.AddScoped<MedicalRecordRepository>();
builder.Services.AddScoped<MedicationRepository>();
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<PrescriptionRepository>();

var app = builder.Build();

// Standard ASP.NET Core Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
