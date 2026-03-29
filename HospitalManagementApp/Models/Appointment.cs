using System;

namespace HospitalManagementApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Room { get; set; }
        public string Notes { get; set; }
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
    }
}
