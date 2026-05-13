using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class DoctorsController : Controller
{
    private readonly DoctorRepository _repo;

    public DoctorsController(DoctorRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("/Doctors")]
    [Route("/staff/doctors")]
    public IActionResult Index(string? term) => View(_repo.GetAll(term));

    [HttpGet]
    public IActionResult Search(string? term) => PartialView("_DoctorList", _repo.GetAll(term));

    [HttpGet("/Doctors/Options")]
    public IActionResult Options(string? term)
    {
        var options = _repo.GetAll(term)
            .Take(20)
            .Select(d => new
            {
                value = d.Id,
                label = "Dr. " + d.LastName + ", " + d.FirstName + " (" + d.Specialty + ")"
            });

        return Json(options);
    }

    public IActionResult Details(int id)
    {
        var doctor = _repo.GetById(id);
        if (doctor == null) return NotFound();
        return View(doctor);
    }

    [HttpGet]
    public IActionResult Create()
    {
        LoadDepartmentSelection();
        return View(new Doctor());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Doctor doctor, int[] selectedDepartmentIds)
    {
        if (!ModelState.IsValid)
        {
            LoadDepartmentSelection(selectedDepartmentIds);
            return View(doctor);
        }

        _repo.Add(doctor, selectedDepartmentIds);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var doctor = _repo.GetByIdForEdit(id);
        if (doctor == null) return NotFound();

        LoadDepartmentSelection(doctor.Departments.Select(d => d.Id));
        return View(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Doctor doctor, int[] selectedDepartmentIds)
    {
        if (id != doctor.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            LoadDepartmentSelection(selectedDepartmentIds);
            return View(doctor);
        }

        if (!_repo.Update(doctor, selectedDepartmentIds)) return NotFound();
        return RedirectToAction(nameof(Details), new { id = doctor.Id });
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var doctor = _repo.GetById(id);
        if (doctor == null) return NotFound();
        ViewBag.CanDelete = _repo.CanDelete(id);
        return View(doctor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_repo.Delete(id))
        {
            TempData["Error"] = "Doctor cannot be deleted while assigned to departments or appointments.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }

    private void LoadDepartmentSelection(IEnumerable<int>? selectedDepartmentIds = null)
    {
        ViewBag.Departments = new MultiSelectList(
            _repo.GetDepartmentsForSelection(),
            "Id",
            "Name",
            selectedDepartmentIds
        );
    }
}
