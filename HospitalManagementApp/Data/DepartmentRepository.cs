using HospitalManagementApp.Models;

namespace HospitalManagementApp.Data;

public class DepartmentRepository
{
    private readonly List<Department> _departments = new()
    {
        MockData.DeptCardiology, MockData.DeptNeurology, MockData.DeptGeneralMed
    };

    public List<Department> GetAll() => _departments;
    public Department? GetById(int id) => _departments.FirstOrDefault(d => d.Id == id);
}
