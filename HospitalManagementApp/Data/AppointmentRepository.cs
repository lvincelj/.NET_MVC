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

    public List<Appointment> GetAll(string? searchTerm = null)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(a =>
                a.Room.Contains(term) ||
                a.Status.ToString().Contains(term) ||
                (a.Patient != null && (a.Patient.FirstName + " " + a.Patient.LastName).Contains(term)) ||
                (a.Doctor != null && (a.Doctor.FirstName + " " + a.Doctor.LastName).Contains(term))
            );
        }

        return query
            .OrderBy(a => a.ScheduledAt)
            .ToList();
    }

    public Appointment? GetById(int id) => _context.Appointments
        .AsNoTracking()
        .Include(a => a.Patient)
        .Include(a => a.Doctor)
        .FirstOrDefault(a => a.Id == id);

    public Appointment? GetByIdForEdit(int id) => _context.Appointments
        .FirstOrDefault(a => a.Id == id);

    public void Add(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        _context.SaveChanges();
    }

    public bool Update(Appointment appointment)
    {
        var existing = _context.Appointments.FirstOrDefault(a => a.Id == appointment.Id);
        if (existing == null)
        {
            return false;
        }

        existing.PatientId = appointment.PatientId;
        existing.DoctorId = appointment.DoctorId;
        existing.ScheduledAt = appointment.ScheduledAt;
        existing.Status = appointment.Status;
        existing.Room = appointment.Room;
        existing.Notes = appointment.Notes;

        _context.SaveChanges();
        return true;
    }

    public bool CanDelete(int id) => _context.Appointments.Any(a => a.Id == id);

    public bool Delete(int id)
    {
        var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
        if (appointment == null)
        {
            return false;
        }

        _context.Appointments.Remove(appointment);
        _context.SaveChanges();
        return true;
    }
}
