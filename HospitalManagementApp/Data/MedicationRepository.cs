using HospitalManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Data;

public class MedicationRepository
{
    private readonly AppDbContext _context;

    public MedicationRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Medication> GetAll(string? searchTerm = null)
    {
        var query = _context.Medications
            .AsNoTracking()
            .Include(m => m.Prescription)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(m =>
                m.Name.Contains(term) ||
                m.Dosage.Contains(term) ||
                (m.Instructions != null && m.Instructions.Contains(term))
            );
        }

        return query
            .OrderBy(m => m.Name)
            .ToList();
    }

    public Medication? GetById(int id) => _context.Medications
        .AsNoTracking()
        .Include(m => m.Prescription)
            .ThenInclude(p => p.MedicalRecord)
        .FirstOrDefault(m => m.Id == id);

    public Medication? GetByIdForEdit(int id) => _context.Medications
        .FirstOrDefault(m => m.Id == id);

    public List<Prescription> GetPrescriptionsForSelection() => _context.Prescriptions
        .AsNoTracking()
        .Include(p => p.MedicalRecord)
            .ThenInclude(r => r.Patient)
        .OrderByDescending(p => p.IssuedAt)
        .ToList();

    public void Add(Medication medication)
    {
        _context.Medications.Add(medication);
        _context.SaveChanges();
    }

    public bool Update(Medication medication)
    {
        var existing = _context.Medications.FirstOrDefault(m => m.Id == medication.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Name = medication.Name;
        existing.Dosage = medication.Dosage;
        existing.Instructions = medication.Instructions;
        existing.PrescriptionId = medication.PrescriptionId;

        _context.SaveChanges();
        return true;
    }

    public bool CanDelete(int id) => _context.Medications.Any(m => m.Id == id);

    public bool Delete(int id)
    {
        var medication = _context.Medications.FirstOrDefault(m => m.Id == id);
        if (medication == null)
        {
            return false;
        }

        _context.Medications.Remove(medication);
        _context.SaveChanges();
        return true;
    }
}
