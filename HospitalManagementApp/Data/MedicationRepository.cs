using HospitalManagementApp.Models;

namespace HospitalManagementApp.Data;

public class MedicationRepository
{
    private readonly List<Medication> _medications = new()
    {
        MockData.Med1, MockData.Med2, MockData.Med3
    };

    public List<Medication> GetAll() => _medications;
    public Medication? GetById(int id) => _medications.FirstOrDefault(m => m.Id == id);
}
