using HospitalManagementApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class PatientsController : Controller
{
    private readonly PatientRepository _repo = new();

    public IActionResult Index() => View(_repo.GetAll());

    public IActionResult Details(int id)
    {
        var patient = _repo.GetById(id);
        if (patient == null) return NotFound();
        return View(patient);
    }
}
