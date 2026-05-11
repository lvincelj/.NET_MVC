using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class PatientsController : Controller
{
    private readonly PatientRepository _repo;

    public PatientsController(PatientRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("/patients")]
    public IActionResult Index(string? term) => View(_repo.GetAll(term));

    [HttpGet]
    public IActionResult Search(string? term) => PartialView("_PatientList", _repo.GetAll(term));

    [HttpGet("/patients/options")]
    public IActionResult Options(string? term)
    {
        var options = _repo.GetAll(term)
            .Take(20)
            .Select(p => new
            {
                value = p.Id,
                label = p.LastName + ", " + p.FirstName
            });

        return Json(options);
    }

    [HttpGet("/patients/{id:int}")]
    public IActionResult Details(int id)
    {
        var patient = _repo.GetById(id);
        if (patient == null) return NotFound();
        return View(patient);
    }

    [HttpGet]
    public IActionResult Create() => View(new Patient());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Patient patient)
    {
        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        _repo.Add(patient);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var patient = _repo.GetByIdForEdit(id);
        if (patient == null) return NotFound();
        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Patient patient)
    {
        if (id != patient.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        if (!_repo.Update(patient)) return NotFound();
        return RedirectToAction(nameof(Details), new { id = patient.Id });
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var patient = _repo.GetById(id);
        if (patient == null) return NotFound();
        ViewBag.CanDelete = _repo.CanDelete(id);
        return View(patient);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_repo.Delete(id))
        {
            TempData["Error"] = "Patient cannot be deleted while appointments or medical records exist.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }
}
