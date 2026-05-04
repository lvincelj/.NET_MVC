using HospitalManagementApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class DoctorsController : Controller
{
    private readonly DoctorRepository _repo;

    public DoctorsController(DoctorRepository repo)
    {
        _repo = repo;
    }

    [Route("/staff/doctors")]
    [HttpGet]
    public IActionResult Index() => View(_repo.GetAll());

    public IActionResult Details(int id)
    {
        var doctor = _repo.GetById(id);
        if (doctor == null) return NotFound();
        return View(doctor);
    }
}
