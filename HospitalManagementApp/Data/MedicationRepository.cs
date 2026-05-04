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

    public List<Medication> GetAll() => _context.Medications
        .AsNoTracking()
        .Include(m => m.Prescription)
        .OrderBy(m => m.Name)
        .ToList();

    public Medication? GetById(int id) => _context.Medications
        .AsNoTracking()
        .Include(m => m.Prescription)
            .ThenInclude(p => p.MedicalRecord)
        .FirstOrDefault(m => m.Id == id);
}
