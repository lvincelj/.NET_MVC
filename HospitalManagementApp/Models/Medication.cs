namespace HospitalManagementApp.Models
{
    public class Medication
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dosage { get; set; }
        public string Instructions { get; set; }
        public int PrescriptionId { get; set; }
        public Prescription Prescription { get; set; }
    }
}
