using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
using HospitalManagementApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class MedicationsController : Controller
{
    private readonly MedicationRepository _repo;

    public MedicationsController(MedicationRepository repo)
    {
        _repo = repo;
    }

    [AllowAnonymous]
    public IActionResult Index(string? term) => View(_repo.GetAll(term));

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Search(string? term) => PartialView("_MedicationList", _repo.GetAll(term));

    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var medication = _repo.GetById(id);
        if (medication == null) return NotFound();
        return View(medication);
    }

    [HttpGet("/Medications/Create")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Create()
    {
        LoadPrescriptionSelection();
        return View(new Medication());
    }

    [HttpPost("/Medications/Create")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Create(Medication medication)
    {
        ValidatePrescriptionReference(medication.PrescriptionId);

        if (!ModelState.IsValid)
        {
            LoadPrescriptionSelection(medication.PrescriptionId);
            return View(medication);
        }

        _repo.Add(medication);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Edit(int id)
    {
        var medication = _repo.GetByIdForEdit(id);
        if (medication == null) return NotFound();

        LoadPrescriptionSelection(medication.PrescriptionId);
        return View(medication);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Doctor)]
    public IActionResult Edit(int id, Medication medication)
    {
        if (id != medication.Id) return NotFound();

        ValidatePrescriptionReference(medication.PrescriptionId);

        if (!ModelState.IsValid)
        {
            LoadPrescriptionSelection(medication.PrescriptionId);
            return View(medication);
        }

        if (!_repo.Update(medication)) return NotFound();
        return RedirectToAction(nameof(Details), new { id = medication.Id });
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult Delete(int id)
    {
        var medication = _repo.GetById(id);
        if (medication == null) return NotFound();
        return View(medication);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_repo.Delete(id)) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private void LoadPrescriptionSelection(int? selectedPrescriptionId = null)
    {
        var prescriptions = _repo.GetPrescriptionsForSelection()
            .Select(p => new
            {
                p.Id,
                Display = "#" + p.Id + " - " + p.IssuedBy + " (" + p.IssuedAt.ToString("yyyy-MM-dd") + ")"
            })
            .ToList();

        ViewBag.PrescriptionId = new SelectList(prescriptions, "Id", "Display", selectedPrescriptionId);
        ViewBag.SelectedPrescriptionText = prescriptions.FirstOrDefault(p => p.Id == selectedPrescriptionId)?.Display ?? string.Empty;
    }

    private void ValidatePrescriptionReference(int prescriptionId)
    {
        if (prescriptionId > 0 && _repo.GetPrescriptionsForSelection().All(p => p.Id != prescriptionId))
        {
            ModelState.AddModelError(nameof(Medication.PrescriptionId), "Selected prescription does not exist.");
        }
    }
}
