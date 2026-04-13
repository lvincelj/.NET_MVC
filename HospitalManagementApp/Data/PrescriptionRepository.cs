using HospitalManagementApp.Models;

namespace HospitalManagementApp.Data;

public class PrescriptionRepository
{
    private readonly List<Prescription> _prescriptions = new()
    {
        MockData.Presc1, MockData.Presc2, MockData.Presc3
    };

    public List<Prescription> GetAll() => _prescriptions;
    public Prescription? GetById(int id) => _prescriptions.FirstOrDefault(p => p.Id == id);
}
