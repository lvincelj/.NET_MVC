using HospitalManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Data;

public class DepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Department> GetAll(string? searchTerm = null)
    {
        var query = _context.Departments
            .AsNoTracking()
            .Include(d => d.Doctors)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(d =>
                d.Name.Contains(term) ||
                d.Location.Contains(term) ||
                (d.HeadOfDepartment != null && d.HeadOfDepartment.Contains(term)) ||
                (d.PhoneNumber != null && d.PhoneNumber.Contains(term))
            );
        }

        return query
            .OrderBy(d => d.Name)
            .ToList();
    }

    public Department? GetById(int id) => _context.Departments
        .AsNoTracking()
        .Include(d => d.Doctors)
        .FirstOrDefault(d => d.Id == id);

    public Department? GetByIdForEdit(int id) => _context.Departments
        .Include(d => d.Doctors)
        .FirstOrDefault(d => d.Id == id);

    public List<Doctor> GetDoctorsForSelection() => _context.Doctors
        .AsNoTracking()
        .OrderBy(d => d.LastName)
        .ThenBy(d => d.FirstName)
        .ToList();

    public void Add(Department department, IEnumerable<int> doctorIds)
    {
        var doctorIdSet = doctorIds.Distinct().ToHashSet();
        department.Doctors = _context.Doctors
            .Where(d => doctorIdSet.Contains(d.Id))
            .ToList();

        _context.Departments.Add(department);
        _context.SaveChanges();
    }

    public bool Update(Department department, IEnumerable<int> doctorIds)
    {
        var existing = _context.Departments
            .Include(d => d.Doctors)
            .FirstOrDefault(d => d.Id == department.Id);

        if (existing == null)
        {
            return false;
        }

        existing.Name = department.Name;
        existing.Location = department.Location;
        existing.PhoneNumber = department.PhoneNumber;
        existing.HeadOfDepartment = department.HeadOfDepartment;

        existing.Doctors.Clear();
        var doctorIdSet = doctorIds.Distinct().ToHashSet();
        var selectedDoctors = _context.Doctors
            .Where(d => doctorIdSet.Contains(d.Id))
            .ToList();
        foreach (var doctor in selectedDoctors)
        {
            existing.Doctors.Add(doctor);
        }

        _context.SaveChanges();
        return true;
    }

    public bool CanDelete(int id) => !_context.Doctors.Any(d => d.Departments.Any(dep => dep.Id == id));

    public bool Delete(int id)
    {
        if (!CanDelete(id))
        {
            return false;
        }

        var department = _context.Departments.FirstOrDefault(d => d.Id == id);
        if (department == null)
        {
            return false;
        }

        _context.Departments.Remove(department);
        _context.SaveChanges();
        return true;
    }
}
