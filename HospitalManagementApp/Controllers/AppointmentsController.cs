using HospitalManagementApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class AppointmentsController : Controller
{
    private readonly AppointmentRepository _repo = new();

    public IActionResult Index() => View(_repo.GetAll());

    public IActionResult Details(int id)
    {
        var appointment = _repo.GetById(id);
        if (appointment == null) return NotFound();
        return View(appointment);
    }
}
