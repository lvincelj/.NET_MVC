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

    public List<Prescription> GetAll() => _context.Prescriptions
        .AsNoTracking()
        .Include(p => p.MedicalRecord)
        .Include(p => p.Medications)
        .OrderByDescending(p => p.IssuedAt)
        .ToList();

    public Prescription? GetById(int id) => _context.Prescriptions
        .AsNoTracking()
        .Include(p => p.MedicalRecord)
            .ThenInclude(r => r.Patient)
        .Include(p => p.Medications)
        .FirstOrDefault(p => p.Id == id);
}
