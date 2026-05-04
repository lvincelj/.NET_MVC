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

    public List<Doctor> GetAll() => _context.Doctors
        .AsNoTracking()
        .Include(d => d.Departments)
        .Include(d => d.Appointments)
        .OrderBy(d => d.LastName)
        .ThenBy(d => d.FirstName)
        .ToList();

    public Doctor? GetById(int id) => _context.Doctors
        .AsNoTracking()
        .Include(d => d.Departments)
        .Include(d => d.Appointments)
            .ThenInclude(a => a.Patient)
        .FirstOrDefault(d => d.Id == id);
}
