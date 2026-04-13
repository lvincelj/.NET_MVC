using HospitalManagementApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class MedicalRecordsController : Controller
{
    private readonly MedicalRecordRepository _repo = new();

    public IActionResult Index() => View(_repo.GetAll());

    public IActionResult Details(int id)
    {
        var record = _repo.GetById(id);
        if (record == null) return NotFound();
        return View(record);
    }
}
