using HospitalManagementApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class MedicalRecordsController : Controller
{
    private readonly MedicalRecordRepository _repo;

    public MedicalRecordsController(MedicalRecordRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("/medical-records")]
    public IActionResult Index() => View(_repo.GetAll());

    [HttpGet("/medical-records/{id:int}")]
    public IActionResult Details(int id)
    {
        var record = _repo.GetById(id);
        if (record == null) return NotFound();
        return View(record);
    }

    [HttpPost("/medical-records/{id:int}/mark-reviewed")]
    [ValidateAntiForgeryToken]
    public IActionResult MarkReviewed(int id)
    {
        var record = _repo.GetById(id);
        if (record == null) return NotFound();

        TempData["StatusMessage"] = $"Medical record {id} marked as reviewed.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
