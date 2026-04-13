using HospitalManagementApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class PrescriptionsController : Controller
{
    private readonly PrescriptionRepository _repo = new();

    public IActionResult Index() => View(_repo.GetAll());

    public IActionResult Details(int id)
    {
        var prescription = _repo.GetById(id);
        if (prescription == null) return NotFound();
        return View(prescription);
    }
}
