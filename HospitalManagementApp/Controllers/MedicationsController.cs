using HospitalManagementApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class MedicationsController : Controller
{
    private readonly MedicationRepository _repo;

    public MedicationsController(MedicationRepository repo)
    {
        _repo = repo;
    }

    public IActionResult Index() => View(_repo.GetAll());

    public IActionResult Details(int id)
    {
        var medication = _repo.GetById(id);
        if (medication == null) return NotFound();
        return View(medication);
    }
}
