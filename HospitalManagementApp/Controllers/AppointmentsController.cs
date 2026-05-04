using HospitalManagementApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class AppointmentsController : Controller
{
    private readonly AppointmentRepository _repo;

    public AppointmentsController(AppointmentRepository repo)
    {
        _repo = repo;
    }

    public IActionResult Index() => View(_repo.GetAll());

    [HttpGet("/schedule/appointments/{id:int}")]
    public IActionResult Details(int id)
    {
        var appointment = _repo.GetById(id);
        if (appointment == null) return NotFound();
        return View(appointment);
    }
}
