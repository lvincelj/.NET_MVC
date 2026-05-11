using HospitalManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Data;

public class DoctorRepository
{
    private readonly AppDbContext _context;

    public DoctorRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Doctor> GetAll(string? searchTerm = null)
    {
        var query = _context.Doctors
            .AsNoTracking()
            .Include(d => d.Departments)
            .Include(d => d.Appointments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(d =>
                d.FirstName.Contains(term) ||
                d.LastName.Contains(term) ||
                d.Specialty.Contains(term) ||
                (d.Email != null && d.Email.Contains(term)) ||
                (d.PhoneNumber != null && d.PhoneNumber.Contains(term))
            );
        }

        return query
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .ToList();
    }

    public Doctor? GetById(int id) => _context.Doctors
        .AsNoTracking()
        .Include(d => d.Departments)
        .Include(d => d.Appointments)
            .ThenInclude(a => a.Patient)
        .FirstOrDefault(d => d.Id == id);

    public Doctor? GetByIdForEdit(int id) => _context.Doctors
        .Include(d => d.Departments)
        .FirstOrDefault(d => d.Id == id);

    public List<Department> GetDepartmentsForSelection() => _context.Departments
        .AsNoTracking()
        .OrderBy(d => d.Name)
        .ToList();

    public void Add(Doctor doctor, IEnumerable<int> departmentIds)
    {
        var departmentIdSet = departmentIds.Distinct().ToHashSet();
        doctor.Departments = _context.Departments
            .Where(d => departmentIdSet.Contains(d.Id))
            .ToList();

        _context.Doctors.Add(doctor);
        _context.SaveChanges();
    }

    public bool Update(Doctor doctor, IEnumerable<int> departmentIds)
    {
        var existing = _context.Doctors
            .Include(d => d.Departments)
            .FirstOrDefault(d => d.Id == doctor.Id);

        if (existing == null)
        {
            return false;
        }

        existing.FirstName = doctor.FirstName;
        existing.LastName = doctor.LastName;
        existing.Gender = doctor.Gender;
        existing.Specialty = doctor.Specialty;
        existing.Email = doctor.Email;
        existing.PhoneNumber = doctor.PhoneNumber;

        existing.Departments.Clear();
        var departmentIdSet = departmentIds.Distinct().ToHashSet();
        var selectedDepartments = _context.Departments
            .Where(d => departmentIdSet.Contains(d.Id))
            .ToList();
        foreach (var department in selectedDepartments)
        {
            existing.Departments.Add(department);
        }

        _context.SaveChanges();
        return true;
    }

    public bool CanDelete(int id) =>
        !_context.Appointments.Any(a => a.DoctorId == id) &&
        !_context.Departments.Any(d => d.Doctors.Any(doc => doc.Id == id));

    public bool Delete(int id)
    {
        if (!CanDelete(id))
        {
            return false;
        }

        var doctor = _context.Doctors.FirstOrDefault(d => d.Id == id);
        if (doctor == null)
        {
            return false;
        }

        _context.Doctors.Remove(doctor);
        _context.SaveChanges();
        return true;
    }
}
