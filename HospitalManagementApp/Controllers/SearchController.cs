using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Controllers;

[AllowAnonymous]
public class SearchController : Controller
{
    private readonly AppDbContext _context;

    public SearchController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("/search")]
    public async Task<IActionResult> Index(string? query)
    {
        var normalizedQuery = NormalizeQuery(query);
        var results = await BuildResultsAsync(normalizedQuery, 10);

        return View(new GlobalSearchViewModel
        {
            Query = normalizedQuery,
            Results = results
        });
    }

    [HttpGet("/search/suggestions")]
    public async Task<IActionResult> Suggestions(string? q)
    {
        var normalizedQuery = NormalizeQuery(q);
        var results = await BuildResultsAsync(normalizedQuery, 4);

        return Json(results.Take(12));
    }

    private async Task<List<GlobalSearchResultItem>> BuildResultsAsync(string query, int perCategory)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return GetNavigationResults(string.Empty)
                .Take(6)
                .ToList();
        }

        var results = new List<GlobalSearchResultItem>();
        results.AddRange(GetNavigationResults(query));

        var likeQuery = $"%{query}%";

        var patients = await _context.Patients
            .Where(p =>
                EF.Functions.Like(p.FirstName, likeQuery) ||
                EF.Functions.Like(p.LastName, likeQuery) ||
                (p.Email != null && EF.Functions.Like(p.Email, likeQuery)) ||
                (p.PhoneNumber != null && EF.Functions.Like(p.PhoneNumber, likeQuery)) ||
                (p.Address != null && EF.Functions.Like(p.Address, likeQuery)))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Take(perCategory)
            .ToListAsync();

        results.AddRange(patients.Select(p => new GlobalSearchResultItem
        {
            Category = "Patients",
            Title = $"{p.FirstName} {p.LastName}",
            Description = string.Join(" · ", new[] { p.Email, p.PhoneNumber, p.Address }.Where(v => !string.IsNullOrWhiteSpace(v))),
            Url = Url.Action("Details", "Patients", new { id = p.Id }) ?? "#",
            Badge = "Patient"
        }));

        var doctors = await _context.Doctors
            .Include(d => d.Departments)
            .Where(d =>
                EF.Functions.Like(d.FirstName, likeQuery) ||
                EF.Functions.Like(d.LastName, likeQuery) ||
                EF.Functions.Like(d.Specialty, likeQuery) ||
                (d.Email != null && EF.Functions.Like(d.Email, likeQuery)) ||
                d.Departments.Any(dep => EF.Functions.Like(dep.Name, likeQuery)))
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .Take(perCategory)
            .ToListAsync();

        results.AddRange(doctors.Select(d => new GlobalSearchResultItem
        {
            Category = "Doctors",
            Title = $"Dr. {d.FirstName} {d.LastName}",
            Description = string.Join(" · ", new[] { d.Specialty, string.Join(", ", d.Departments.Select(dep => dep.Name)) }.Where(v => !string.IsNullOrWhiteSpace(v))),
            Url = Url.Action("Details", "Doctors", new { id = d.Id }) ?? "#",
            Badge = "Doctor"
        }));

        var departments = await _context.Departments
            .Where(d =>
                EF.Functions.Like(d.Name, likeQuery) ||
                EF.Functions.Like(d.Location, likeQuery) ||
                (d.HeadOfDepartment != null && EF.Functions.Like(d.HeadOfDepartment, likeQuery)) ||
                (d.PhoneNumber != null && EF.Functions.Like(d.PhoneNumber, likeQuery)))
            .OrderBy(d => d.Name)
            .Take(perCategory)
            .ToListAsync();

        results.AddRange(departments.Select(d => new GlobalSearchResultItem
        {
            Category = "Departments",
            Title = d.Name,
            Description = string.Join(" · ", new[] { d.Location, d.HeadOfDepartment, d.PhoneNumber }.Where(v => !string.IsNullOrWhiteSpace(v))),
            Url = Url.Action("Details", "Departments", new { id = d.Id }) ?? "#",
            Badge = "Department"
        }));

        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a =>
                EF.Functions.Like(a.Room, likeQuery) ||
                (a.Notes != null && EF.Functions.Like(a.Notes, likeQuery)) ||
                EF.Functions.Like(a.Patient.FirstName, likeQuery) ||
                EF.Functions.Like(a.Patient.LastName, likeQuery) ||
                EF.Functions.Like(a.Doctor.FirstName, likeQuery) ||
                EF.Functions.Like(a.Doctor.LastName, likeQuery))
            .OrderBy(a => a.ScheduledAt)
            .Take(perCategory)
            .ToListAsync();

        results.AddRange(appointments.Select(a => new GlobalSearchResultItem
        {
            Category = "Appointments",
            Title = $"{a.Patient.FirstName} {a.Patient.LastName} with Dr. {a.Doctor.LastName}",
            Description = $"{a.ScheduledAt:g} · Room {a.Room} · {a.Status}",
            Url = Url.Action("Details", "Appointments", new { id = a.Id }) ?? "#",
            Badge = "Appointment"
        }));

        var medicalRecords = await _context.MedicalRecords
            .Include(r => r.Patient)
            .Where(r =>
                EF.Functions.Like(r.Diagnosis, likeQuery) ||
                (r.Notes != null && EF.Functions.Like(r.Notes, likeQuery)) ||
                EF.Functions.Like(r.Patient.FirstName, likeQuery) ||
                EF.Functions.Like(r.Patient.LastName, likeQuery))
            .OrderByDescending(r => r.CreatedAt)
            .Take(perCategory)
            .ToListAsync();

        results.AddRange(medicalRecords.Select(r => new GlobalSearchResultItem
        {
            Category = "Medical Records",
            Title = r.Diagnosis,
            Description = $"{r.Patient.FirstName} {r.Patient.LastName} · {r.CreatedAt:d}",
            Url = Url.Action("Details", "MedicalRecords", new { id = r.Id }) ?? "#",
            Badge = "Record"
        }));

        var prescriptions = await _context.Prescriptions
            .Include(p => p.MedicalRecord)
            .ThenInclude(r => r.Patient)
            .Where(p =>
                EF.Functions.Like(p.IssuedBy, likeQuery) ||
                EF.Functions.Like(p.MedicalRecord.Diagnosis, likeQuery) ||
                EF.Functions.Like(p.MedicalRecord.Patient.FirstName, likeQuery) ||
                EF.Functions.Like(p.MedicalRecord.Patient.LastName, likeQuery))
            .OrderByDescending(p => p.IssuedAt)
            .Take(perCategory)
            .ToListAsync();

        results.AddRange(prescriptions.Select(p => new GlobalSearchResultItem
        {
            Category = "Prescriptions",
            Title = $"Prescription by {p.IssuedBy}",
            Description = $"{p.MedicalRecord.Patient.FirstName} {p.MedicalRecord.Patient.LastName} · {p.IssuedAt:d}",
            Url = Url.Action("Details", "Prescriptions", new { id = p.Id }) ?? "#",
            Badge = "Prescription"
        }));

        var medications = await _context.Medications
            .Include(m => m.Prescription)
            .ThenInclude(p => p.MedicalRecord)
            .ThenInclude(r => r.Patient)
            .Where(m =>
                EF.Functions.Like(m.Name, likeQuery) ||
                EF.Functions.Like(m.Dosage, likeQuery) ||
                (m.Instructions != null && EF.Functions.Like(m.Instructions, likeQuery)) ||
                EF.Functions.Like(m.Prescription.MedicalRecord.Patient.FirstName, likeQuery) ||
                EF.Functions.Like(m.Prescription.MedicalRecord.Patient.LastName, likeQuery))
            .OrderBy(m => m.Name)
            .Take(perCategory)
            .ToListAsync();

        results.AddRange(medications.Select(m => new GlobalSearchResultItem
        {
            Category = "Medications",
            Title = m.Name,
            Description = $"{m.Dosage} · {m.Prescription.MedicalRecord.Patient.FirstName} {m.Prescription.MedicalRecord.Patient.LastName}",
            Url = Url.Action("Details", "Medications", new { id = m.Id }) ?? "#",
            Badge = "Medication"
        }));

        return results;
    }

    private IEnumerable<GlobalSearchResultItem> GetNavigationResults(string query)
    {
        var navigationItems = new[]
        {
            new { Result = CreateNavigationResult("Dashboard", "Operational overview, KPIs, quick actions", "Home", "Index", "Page"), Keywords = "dashboard overview home" },
            new { Result = CreateNavigationResult("Patients", "Patient profiles, contact details, medical history", "Patients", "Index", "Menu"), Keywords = "patients pacijenti people profiles" },
            new { Result = CreateNavigationResult("Doctors", "Doctor roster, specialties, department coverage", "Doctors", "Index", "Menu"), Keywords = "doctors lijecnici staff specialties" },
            new { Result = CreateNavigationResult("Departments", "Hospital departments, locations, leadership", "Departments", "Index", "Menu"), Keywords = "departments odjeli locations" },
            new { Result = CreateNavigationResult("Appointments", "Schedule, rooms, visits and appointment status", "Appointments", "Index", "Menu"), Keywords = "appointments termini schedule rooms" },
            new { Result = CreateNavigationResult("Medical Records", "Diagnoses, clinical notes and patient records", "MedicalRecords", "Index", "Menu"), Keywords = "medical records kartoni diagnosis notes" },
            new { Result = CreateNavigationResult("Medications", "Medication catalog, dosage and instructions", "Medications", "Index", "Menu"), Keywords = "medications lijekovi dosage" },
            new { Result = CreateNavigationResult("Prescriptions", "Issued prescriptions and linked medications", "Prescriptions", "Index", "Menu"), Keywords = "prescriptions recepti issued medications" },
            new { Result = CreateNavigationResult("Login", "Sign in to CareFlow", "Account", "Login", "Page"), Keywords = "login sign in" },
            new { Result = CreateNavigationResult("Register", "Create a CareFlow account", "Account", "Register", "Page"), Keywords = "register create account" }
        };

        if (string.IsNullOrWhiteSpace(query))
        {
            return navigationItems.Select(item => item.Result);
        }

        return navigationItems.Where(item =>
                Contains(item.Result.Title, query) ||
                Contains(item.Result.Description, query) ||
                Contains(item.Result.Category, query) ||
                Contains(item.Result.Badge, query) ||
                Contains(item.Keywords, query))
            .Select(item => item.Result);
    }

    private GlobalSearchResultItem CreateNavigationResult(
        string title,
        string description,
        string controller,
        string action,
        string badge)
    {
        return new GlobalSearchResultItem
        {
            Category = "Navigation",
            Title = title,
            Description = description,
            Url = Url.Action(action, controller) ?? "#",
            Badge = badge
        };
    }

    private static bool Contains(string? value, string query) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Contains(query, StringComparison.OrdinalIgnoreCase);

    private static string NormalizeQuery(string? query) =>
        string.Join(" ", (query ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
