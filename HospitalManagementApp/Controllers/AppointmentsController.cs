using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
using HospitalManagementApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class AppointmentsController : Controller
{
    private readonly AppointmentRepository _repo;
    private readonly PatientRepository _patientRepo;
    private readonly DoctorRepository _doctorRepo;

    public AppointmentsController(AppointmentRepository repo, PatientRepository patientRepo, DoctorRepository doctorRepo)
    {
        _repo = repo;
        _patientRepo = patientRepo;
        _doctorRepo = doctorRepo;
    }

    [AllowAnonymous]
    public IActionResult Index(string? term) => View(_repo.GetAll(term));

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Search(string? term) => PartialView("_AppointmentList", _repo.GetAll(term));

    [HttpGet("/Appointments/Details/{id:int}")]
    [HttpGet("/schedule/appointments/{id:int}")]
    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var appointment = _repo.GetById(id);
        if (appointment == null) return NotFound();
        return View(appointment);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Create()
    {
        LoadLookupLists();
        return View(new Appointment { ScheduledAt = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Create(Appointment appointment)
    {
        if (!ModelState.IsValid)
        {
            LoadLookupLists(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }

        _repo.Add(appointment);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Edit(int id)
    {
        var appointment = _repo.GetByIdForEdit(id);
        if (appointment == null) return NotFound();

        LoadLookupLists(appointment.PatientId, appointment.DoctorId);
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Edit(int id, Appointment appointment)
    {
        if (id != appointment.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            LoadLookupLists(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }

        if (!_repo.Update(appointment)) return NotFound();
        return RedirectToAction(nameof(Details), new { id = appointment.Id });
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult Delete(int id)
    {
        var appointment = _repo.GetById(id);
        if (appointment == null) return NotFound();
        return View(appointment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_repo.Delete(id)) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private void LoadLookupLists(int? selectedPatientId = null, int? selectedDoctorId = null)
    {
        var patients = _patientRepo.GetAll()
            .Select(p => new { p.Id, FullName = p.LastName + ", " + p.FirstName })
            .OrderBy(p => p.FullName)
            .ToList();

        var doctors = _doctorRepo.GetAll()
            .Select(d => new { d.Id, FullName = "Dr. " + d.LastName + ", " + d.FirstName })
            .OrderBy(d => d.FullName)
            .ToList();

        ViewBag.PatientId = new SelectList(patients, "Id", "FullName", selectedPatientId);
        ViewBag.DoctorId = new SelectList(doctors, "Id", "FullName", selectedDoctorId);
        ViewBag.SelectedPatientText = patients.FirstOrDefault(p => p.Id == selectedPatientId)?.FullName ?? string.Empty;
        ViewBag.SelectedDoctorText = doctors.FirstOrDefault(d => d.Id == selectedDoctorId)?.FullName ?? string.Empty;
    }
}
