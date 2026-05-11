using HospitalManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Data;

public class PrescriptionRepository
{
    private readonly AppDbContext _context;

    public PrescriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Prescription> GetAll(string? searchTerm = null)
    {
        var query = _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.MedicalRecord)
                .ThenInclude(r => r.Patient)
            .Include(p => p.Medications)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(p =>
                p.IssuedBy.Contains(term) ||
                (p.MedicalRecord != null && p.MedicalRecord.Diagnosis.Contains(term)) ||
                (p.MedicalRecord != null && p.MedicalRecord.Patient != null &&
                 (p.MedicalRecord.Patient.FirstName + " " + p.MedicalRecord.Patient.LastName).Contains(term))
            );
        }

        return query
            .OrderByDescending(p => p.IssuedAt)
            .ToList();
    }

    public Prescription? GetById(int id) => _context.Prescriptions
        .AsNoTracking()
        .Include(p => p.MedicalRecord)
            .ThenInclude(r => r.Patient)
        .Include(p => p.Medications)
        .FirstOrDefault(p => p.Id == id);

    public Prescription? GetByIdForEdit(int id) => _context.Prescriptions
        .FirstOrDefault(p => p.Id == id);

    public List<MedicalRecord> GetMedicalRecordsForSelection() => _context.MedicalRecords
        .AsNoTracking()
        .Include(r => r.Patient)
        .OrderByDescending(r => r.CreatedAt)
        .ToList();

    public void Add(Prescription prescription)
    {
        _context.Prescriptions.Add(prescription);
        _context.SaveChanges();
    }

    public bool Update(Prescription prescription)
    {
        var existing = _context.Prescriptions.FirstOrDefault(p => p.Id == prescription.Id);
        if (existing == null)
        {
            return false;
        }

        existing.MedicalRecordId = prescription.MedicalRecordId;
        existing.IssuedAt = prescription.IssuedAt;
        existing.IssuedBy = prescription.IssuedBy;

        _context.SaveChanges();
        return true;
    }

    public bool CanDelete(int id) => !_context.Medications.Any(m => m.PrescriptionId == id);

    public bool Delete(int id)
    {
        if (!CanDelete(id))
        {
            return false;
        }

        var prescription = _context.Prescriptions.FirstOrDefault(p => p.Id == id);
        if (prescription == null)
        {
            return false;
        }

        _context.Prescriptions.Remove(prescription);
        _context.SaveChanges();
        return true;
    }
}
