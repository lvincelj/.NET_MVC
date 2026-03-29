using System.Collections.Generic;

namespace HospitalManagementApp.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public string Specialty { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<Department> Departments { get; set; } = new();
        public List<Appointment> Appointments { get; set; } = new();
    }
}
