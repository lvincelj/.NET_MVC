using HospitalManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Data;

public class MedicalRecordRepository
{
    private readonly AppDbContext _context;

    public MedicalRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<MedicalRecord> GetAll(string? searchTerm = null)
    {
        var query = _context.MedicalRecords
            .AsNoTracking()
            .Include(r => r.Patient)
            .Include(r => r.Prescriptions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(r =>
                r.Diagnosis.Contains(term) ||
                (r.Notes != null && r.Notes.Contains(term)) ||
                (r.Patient != null && (r.Patient.FirstName + " " + r.Patient.LastName).Contains(term))
            );
        }

        return query
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public MedicalRecord? GetById(int id) => _context.MedicalRecords
        .AsNoTracking()
        .Include(r => r.Patient)
        .Include(r => r.Prescriptions)
            .ThenInclude(p => p.Medications)
        .FirstOrDefault(r => r.Id == id);

    public MedicalRecord? GetByIdForEdit(int id) => _context.MedicalRecords
        .FirstOrDefault(r => r.Id == id);

    public List<Patient> GetPatientsForSelection() => _context.Patients
        .AsNoTracking()
        .OrderBy(p => p.LastName)
        .ThenBy(p => p.FirstName)
        .ToList();

    public void Add(MedicalRecord record)
    {
        _context.MedicalRecords.Add(record);
        _context.SaveChanges();
    }

    public bool Update(MedicalRecord record)
    {
        var existing = _context.MedicalRecords.FirstOrDefault(r => r.Id == record.Id);
        if (existing == null)
        {
            return false;
        }

        existing.PatientId = record.PatientId;
        existing.CreatedAt = record.CreatedAt;
        existing.Diagnosis = record.Diagnosis;
        existing.Notes = record.Notes;

        _context.SaveChanges();
        return true;
    }

    public bool CanDelete(int id) => !_context.Prescriptions.Any(p => p.MedicalRecordId == id);

    public bool Delete(int id)
    {
        if (!CanDelete(id))
        {
            return false;
        }

        var record = _context.MedicalRecords.FirstOrDefault(r => r.Id == id);
        if (record == null)
        {
            return false;
        }

        _context.MedicalRecords.Remove(record);
        _context.SaveChanges();
        return true;
    }
}
