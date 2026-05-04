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

    public List<MedicalRecord> GetAll() => _context.MedicalRecords
        .AsNoTracking()
        .Include(r => r.Patient)
        .Include(r => r.Prescriptions)
        .OrderByDescending(r => r.CreatedAt)
        .ToList();

    public MedicalRecord? GetById(int id) => _context.MedicalRecords
        .AsNoTracking()
        .Include(r => r.Patient)
        .Include(r => r.Prescriptions)
            .ThenInclude(p => p.Medications)
        .FirstOrDefault(r => r.Id == id);
}
