using System;
using System.Collections.Generic;

namespace HospitalManagementApp.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Diagnosis { get; set; }
        public string Notes { get; set; }
        public List<Prescription> Prescriptions { get; set; } = new();
        public Patient Patient { get; set; }
    }
}
