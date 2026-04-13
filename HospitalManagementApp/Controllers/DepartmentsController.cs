using HospitalManagementApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class DepartmentsController : Controller
{
    private readonly DepartmentRepository _repo = new();

    public IActionResult Index() => View(_repo.GetAll());

    public IActionResult Details(int id)
    {
        var department = _repo.GetById(id);
        if (department == null) return NotFound();
        return View(department);
    }
}
