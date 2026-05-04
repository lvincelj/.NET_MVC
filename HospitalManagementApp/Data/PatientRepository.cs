using HospitalManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Data;

public class PatientRepository
{
    private readonly AppDbContext _context;

    public PatientRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Patient> GetAll() => _context.Patients
        .AsNoTracking()
        .Include(p => p.Appointments)
        .Include(p => p.MedicalRecords)
        .OrderBy(p => p.LastName)
        .ThenBy(p => p.FirstName)
        .ToList();

    public Patient? GetById(int id) => _context.Patients
        .AsNoTracking()
        .Include(p => p.Appointments)
            .ThenInclude(a => a.Doctor)
        .Include(p => p.MedicalRecords)
            .ThenInclude(r => r.Prescriptions)
                .ThenInclude(pr => pr.Medications)
        .FirstOrDefault(p => p.Id == id);
}
