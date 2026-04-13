using HospitalManagementApp.Models;

namespace HospitalManagementApp.Data;

public class MedicalRecordRepository
{
    private readonly List<MedicalRecord> _records = new()
    {
        MockData.Mr1, MockData.Mr2, MockData.Mr3
    };

    public List<MedicalRecord> GetAll() => _records;
    public MedicalRecord? GetById(int id) => _records.FirstOrDefault(r => r.Id == id);
}
