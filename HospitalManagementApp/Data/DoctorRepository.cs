using HospitalManagementApp.Models;

namespace HospitalManagementApp.Data;

public class DoctorRepository
{
    private readonly List<Doctor> _doctors = new()
    {
        MockData.Doc1, MockData.Doc2, MockData.Doc3
    };

    public List<Doctor> GetAll() => _doctors;
    public Doctor? GetById(int id) => _doctors.FirstOrDefault(d => d.Id == id);
}
