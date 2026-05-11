using HospitalManagementApp.Data;
using HospitalManagementApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

public class DepartmentsController : Controller
{
    private readonly DepartmentRepository _repo;

    public DepartmentsController(DepartmentRepository repo)
    {
        _repo = repo;
    }

    public IActionResult Index(string? term) => View(_repo.GetAll(term));

    [HttpGet]
    public IActionResult Search(string? term) => PartialView("_DepartmentList", _repo.GetAll(term));

    public IActionResult Details(int id)
    {
        var department = _repo.GetById(id);
        if (department == null) return NotFound();
        return View(department);
    }

    [HttpGet]
    public IActionResult Create()
    {
        LoadDoctorSelection();
        return View(new Department());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Department department, int[] selectedDoctorIds)
    {
        if (!ModelState.IsValid)
        {
            LoadDoctorSelection(selectedDoctorIds);
            return View(department);
        }

        _repo.Add(department, selectedDoctorIds);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var department = _repo.GetByIdForEdit(id);
        if (department == null) return NotFound();

        LoadDoctorSelection(department.Doctors.Select(d => d.Id));
        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Department department, int[] selectedDoctorIds)
    {
        if (id != department.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            LoadDoctorSelection(selectedDoctorIds);
            return View(department);
        }

        if (!_repo.Update(department, selectedDoctorIds)) return NotFound();
        return RedirectToAction(nameof(Details), new { id = department.Id });
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var department = _repo.GetById(id);
        if (department == null) return NotFound();
        ViewBag.CanDelete = _repo.CanDelete(id);
        return View(department);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_repo.Delete(id))
        {
            TempData["Error"] = "Department cannot be deleted while doctors are assigned.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }

    private void LoadDoctorSelection(IEnumerable<int>? selectedDoctorIds = null)
    {
        ViewBag.Doctors = new MultiSelectList(
            _repo.GetDoctorsForSelection().Select(d => new { d.Id, FullName = "Dr. " + d.LastName + ", " + d.FirstName }),
            "Id",
            "FullName",
            selectedDoctorIds
        );
    }
}
