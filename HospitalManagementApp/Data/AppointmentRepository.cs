using HospitalManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Data;

public class AppointmentRepository
{
    private readonly AppDbContext _context;

    public AppointmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Appointment> GetAll() => _context.Appointments
        .AsNoTracking()
        .Include(a => a.Patient)
        .Include(a => a.Doctor)
        .OrderBy(a => a.ScheduledAt)
        .ToList();

    public Appointment? GetById(int id) => _context.Appointments
        .AsNoTracking()
        .Include(a => a.Patient)
        .Include(a => a.Doctor)
        .FirstOrDefault(a => a.Id == id);
}
