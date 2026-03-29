using System;
using System.Collections.Generic;

namespace HospitalManagementApp.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public int MedicalRecordId { get; set; }
        public DateTime IssuedAt { get; set; }
        public string IssuedBy { get; set; }
        public List<Medication> Medications { get; set; } = new();
        public MedicalRecord MedicalRecord { get; set; }
    }
}
