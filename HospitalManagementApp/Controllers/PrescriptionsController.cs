using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
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

    public IActionResult Index(string? term) => View(_repo.GetAll(term));

    [HttpGet]
    public IActionResult Search(string? term) => PartialView("_PrescriptionList", _repo.GetAll(term));

    [HttpGet("/prescriptions/options")]
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

    public IActionResult Details(int id)
    {
        var prescription = _repo.GetById(id);
        if (prescription == null) return NotFound();
        return View(prescription);
    }

    [HttpGet]
    public IActionResult Create()
    {
        LoadMedicalRecordSelection();
        return View(new Prescription { IssuedAt = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Prescription prescription)
    {
        if (!ModelState.IsValid)
        {
            LoadMedicalRecordSelection(prescription.MedicalRecordId);
            return View(prescription);
        }

        _repo.Add(prescription);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var prescription = _repo.GetByIdForEdit(id);
        if (prescription == null) return NotFound();

        LoadMedicalRecordSelection(prescription.MedicalRecordId);
        return View(prescription);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Prescription prescription)
    {
        if (id != prescription.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            LoadMedicalRecordSelection(prescription.MedicalRecordId);
            return View(prescription);
        }

        if (!_repo.Update(prescription)) return NotFound();
        return RedirectToAction(nameof(Details), new { id = prescription.Id });
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var prescription = _repo.GetById(id);
        if (prescription == null) return NotFound();
        ViewBag.CanDelete = _repo.CanDelete(id);
        return View(prescription);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_repo.Delete(id))
        {
            TempData["Error"] = "Prescription cannot be deleted while medications reference it.";
            return RedirectToAction(nameof(Delete), new { id });
        }

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
}
