using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
using HospitalManagementApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class PrescriptionsController : Controller
{
    private readonly PrescriptionRepository _repo;

    public PrescriptionsController(PrescriptionRepository repo)
    {
        _repo = repo;
    }

    [AllowAnonymous]
    public IActionResult Index(string? term) => View(_repo.GetAll(term));

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Search(string? term) => PartialView("_PrescriptionList", _repo.GetAll(term));

    [HttpGet("/prescriptions/options")]
    [AllowAnonymous]
    public IActionResult Options(string? term)
    {
        var options = _repo.GetAll(term)
            .Take(20)
            .Select(p => new
            {
                value = p.Id,
                label = "#" + p.Id + " - " + p.IssuedBy + " (" + p.IssuedAt.ToString("yyyy-MM-dd") + ")"
            });

        return Json(options);
    }

    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var prescription = _repo.GetById(id);
        if (prescription == null) return NotFound();
        return View(prescription);
    }

    [HttpGet("/Prescriptions/Create")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Create(int? medicalRecordId = null)
    {
        LoadMedicalRecordSelection(medicalRecordId);
        return View(new Prescription
        {
            IssuedAt = DateTime.Now,
            MedicalRecordId = medicalRecordId ?? 0
        });
    }

    [HttpPost("/Prescriptions/Create")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Create(Prescription prescription)
    {
        ValidateMedicalRecordReference(prescription.MedicalRecordId);

        if (!ModelState.IsValid)
        {
            LoadMedicalRecordSelection(prescription.MedicalRecordId);
            return View(prescription);
        }

        _repo.Add(prescription);
        return RedirectToAction("Details", "MedicalRecords", new { id = prescription.MedicalRecordId });
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Edit(int id)
    {
        var prescription = _repo.GetByIdForEdit(id);
        if (prescription == null) return NotFound();

        LoadMedicalRecordSelection(prescription.MedicalRecordId);
        return View(prescription);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Edit(int id, Prescription prescription)
    {
        if (id != prescription.Id) return NotFound();

        ValidateMedicalRecordReference(prescription.MedicalRecordId);

        if (!ModelState.IsValid)
        {
            LoadMedicalRecordSelection(prescription.MedicalRecordId);
            return View(prescription);
        }

        if (!_repo.Update(prescription)) return NotFound();
        return RedirectToAction(nameof(Details), new { id = prescription.Id });
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult Delete(int id)
    {
        var prescription = _repo.GetById(id);
        if (prescription == null) return NotFound();
        ViewBag.CanDelete = _repo.CanDelete(id);
        return View(prescription);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_repo.Delete(id)) return NotFound();

        return RedirectToAction(nameof(Index));
    }

    private void LoadMedicalRecordSelection(int? selectedMedicalRecordId = null)
    {
        var records = _repo.GetMedicalRecordsForSelection()
            .Select(r => new
            {
                r.Id,
                Display = "Record #" + r.Id + " - " + (r.Patient != null ? (r.Patient.LastName + ", " + r.Patient.FirstName) : "Unknown")
            })
            .ToList();

        ViewBag.MedicalRecordId = new SelectList(records, "Id", "Display", selectedMedicalRecordId);
        ViewBag.SelectedMedicalRecordText = records.FirstOrDefault(r => r.Id == selectedMedicalRecordId)?.Display ?? string.Empty;
    }

    private void ValidateMedicalRecordReference(int medicalRecordId)
    {
        if (medicalRecordId > 0 && _repo.GetMedicalRecordsForSelection().All(r => r.Id != medicalRecordId))
        {
            ModelState.AddModelError(nameof(Prescription.MedicalRecordId), "Selected medical record does not exist.");
        }
    }
}
