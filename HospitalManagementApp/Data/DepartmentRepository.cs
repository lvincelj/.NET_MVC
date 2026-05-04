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

    public List<Department> GetAll() => _context.Departments
        .AsNoTracking()
        .Include(d => d.Doctors)
        .OrderBy(d => d.Name)
        .ToList();

    public Department? GetById(int id) => _context.Departments
        .AsNoTracking()
        .Include(d => d.Doctors)
        .FirstOrDefault(d => d.Id == id);
}
