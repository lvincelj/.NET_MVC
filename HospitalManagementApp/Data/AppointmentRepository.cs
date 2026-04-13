using HospitalManagementApp.Models;

namespace HospitalManagementApp.Data;

public class AppointmentRepository
{
    private readonly List<Appointment> _appointments = new()
    {
        MockData.Appt1, MockData.Appt2, MockData.Appt3
    };

    public List<Appointment> GetAll() => _appointments;
    public Appointment? GetById(int id) => _appointments.FirstOrDefault(a => a.Id == id);
}
