using HospitalManagementApp.Models;

namespace HospitalManagementApp.Data;

public class PatientRepository
{
    private readonly List<Patient> _patients = new()
    {
        MockData.Pat1, MockData.Pat2, MockData.Pat3
    };

    public List<Patient> GetAll() => _patients;
    public Patient? GetById(int id) => _patients.FirstOrDefault(p => p.Id == id);
}
