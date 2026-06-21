using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
using HospitalManagementApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class MedicalRecordsController : Controller
{
    private readonly MedicalRecordRepository _repo;

    public MedicalRecordsController(MedicalRecordRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("/MedicalRecords")]
    [HttpGet("/medical-records")]
    [AllowAnonymous]
    public IActionResult Index(string? term) => View(_repo.GetAll(term));

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Search(string? term) => PartialView("_MedicalRecordList", _repo.GetAll(term));

    [HttpGet("/MedicalRecords/Options")]
    [HttpGet("/medical-records/options")]
    [AllowAnonymous]
    public IActionResult Options(string? term)
    {
        var options = _repo.GetAll(term)
            .Take(20)
            .Select(r => new
            {
                value = r.Id,
                label = "Record #" + r.Id + " - " + r.Diagnosis
            });

        return Json(options);
    }

    [HttpGet("/MedicalRecords/Details/{id:int}")]
    [HttpGet("/medical-records/{id:int}")]
    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var record = _repo.GetById(id);
        if (record == null) return NotFound();
        return View(record);
    }

    [HttpPost("/medical-records/{id:int}/mark-reviewed")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult MarkReviewed(int id)
    {
        var record = _repo.GetById(id);
        if (record == null) return NotFound();

        TempData["StatusMessage"] = $"Medical record {id} marked as reviewed.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("/MedicalRecords/Create")]
    [HttpGet("/medical-records/create")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Create()
    {
        LoadPatientSelection();
        return View(new MedicalRecord { CreatedAt = DateTime.Now });
    }

    [HttpPost("/MedicalRecords/Create")]
    [HttpPost("/medical-records/create")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Create(MedicalRecord record)
    {
        if (!ModelState.IsValid)
        {
            LoadPatientSelection(record.PatientId);
            return View(record);
        }

        _repo.Add(record);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/MedicalRecords/Edit/{id:int}")]
    [HttpGet("/medical-records/{id:int}/edit")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Edit(int id)
    {
        var record = _repo.GetByIdForEdit(id);
        if (record == null) return NotFound();

        LoadPatientSelection(record.PatientId);
        return View(record);
    }

    [HttpPost("/MedicalRecords/Edit/{id:int}")]
    [HttpPost("/medical-records/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Edit(int id, MedicalRecord record)
    {
        if (id != record.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            LoadPatientSelection(record.PatientId);
            return View(record);
        }

        if (!_repo.Update(record)) return NotFound();
        return RedirectToAction(nameof(Details), new { id = record.Id });
    }

    [HttpGet("/MedicalRecords/Delete/{id:int}")]
    [HttpGet("/medical-records/{id:int}/delete")]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult Delete(int id)
    {
        var record = _repo.GetById(id);
        if (record == null) return NotFound();
        ViewBag.CanDelete = _repo.CanDelete(id);
        return View(record);
    }

    [HttpPost("/MedicalRecords/Delete/{id:int}")]
    [HttpPost("/medical-records/{id:int}/delete")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_repo.Delete(id))
        {
            TempData["Error"] = "Medical record cannot be deleted while prescriptions exist.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }

    private void LoadPatientSelection(int? selectedPatientId = null)
    {
        var patients = _repo.GetPatientsForSelection()
            .Select(p => new { p.Id, FullName = p.LastName + ", " + p.FirstName })
            .ToList();

        ViewBag.PatientId = new SelectList(patients, "Id", "FullName", selectedPatientId);
        ViewBag.SelectedPatientText = patients.FirstOrDefault(p => p.Id == selectedPatientId)?.FullName ?? string.Empty;
    }
}
