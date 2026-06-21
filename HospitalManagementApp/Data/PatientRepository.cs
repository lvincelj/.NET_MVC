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

    public List<Patient> GetAll(string? searchTerm = null)
    {
        var query = _context.Patients
            .AsNoTracking()
            .Include(p => p.Appointments)
            .Include(p => p.MedicalRecords)
            .Include(p => p.PacijentDatoteke)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(p =>
                p.FirstName.Contains(term) ||
                p.LastName.Contains(term) ||
                (p.Email != null && p.Email.Contains(term)) ||
                (p.PhoneNumber != null && p.PhoneNumber.Contains(term)) ||
                (p.Address != null && p.Address.Contains(term))
            );
        }

        return query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToList();
    }

    public Patient? GetById(int id) => _context.Patients
        .AsNoTracking()
        .Include(p => p.Appointments)
            .ThenInclude(a => a.Doctor)
        .Include(p => p.MedicalRecords)
            .ThenInclude(r => r.Prescriptions)
                .ThenInclude(pr => pr.Medications)
        .Include(p => p.PacijentDatoteke)
        .FirstOrDefault(p => p.Id == id);

    public Patient? GetByIdForEdit(int id) => _context.Patients
        .FirstOrDefault(p => p.Id == id);

    public void Add(Patient patient)
    {
        _context.Patients.Add(patient);
        _context.SaveChanges();
    }

    public bool Update(Patient patient)
    {
        var existing = _context.Patients.FirstOrDefault(p => p.Id == patient.Id);
        if (existing == null)
        {
            return false;
        }

        existing.FirstName = patient.FirstName;
        existing.LastName = patient.LastName;
        existing.Gender = patient.Gender;
        existing.DateOfBirth = patient.DateOfBirth;
        existing.Email = patient.Email;
        existing.PhoneNumber = patient.PhoneNumber;
        existing.Address = patient.Address;

        _context.SaveChanges();
        return true;
    }

    public bool CanDelete(int id) => _context.Patients.Any(p => p.Id == id);

    public bool Delete(int id)
    {
        var patient = _context.Patients
            .Include(p => p.Appointments)
            .Include(p => p.MedicalRecords)
                .ThenInclude(r => r.Prescriptions)
                    .ThenInclude(pr => pr.Medications)
            .Include(p => p.PacijentDatoteke)
            .FirstOrDefault(p => p.Id == id);
        if (patient == null)
        {
            return false;
        }

        var prescriptions = patient.MedicalRecords.SelectMany(r => r.Prescriptions).ToList();
        _context.Medications.RemoveRange(prescriptions.SelectMany(p => p.Medications));
        _context.Prescriptions.RemoveRange(prescriptions);
        _context.MedicalRecords.RemoveRange(patient.MedicalRecords);
        _context.Appointments.RemoveRange(patient.Appointments);
        _context.PacijentDatoteke.RemoveRange(patient.PacijentDatoteke);
        _context.Patients.Remove(patient);
        _context.SaveChanges();
        return true;
    }
}
