using System.Collections.Generic;

namespace HospitalManagementApp.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string PhoneNumber { get; set; }
        public string HeadOfDepartment { get; set; }
        public List<Doctor> Doctors { get; set; } = new();
    }
}
