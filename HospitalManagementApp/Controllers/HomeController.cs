using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HospitalManagementApp.Data;
using HospitalManagementApp.Models;

namespace HospitalManagementApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly PatientRepository _patientRepository = new();
        private readonly DoctorRepository _doctorRepository = new();
        private readonly DepartmentRepository _departmentRepository = new();
        private readonly AppointmentRepository _appointmentRepository = new();
        private readonly MedicalRecordRepository _medicalRecordRepository = new();
        private readonly MedicationRepository _medicationRepository = new();

        public IActionResult Index()
        {
            var patients = _patientRepository.GetAll();
            var doctors = _doctorRepository.GetAll();
            var departments = _departmentRepository.GetAll();
            var appointments = _appointmentRepository.GetAll().OrderBy(a => a.ScheduledAt).ToList();
            var medicalRecords = _medicalRecordRepository.GetAll();
            var medications = _medicationRepository.GetAll();
            var nextAppointment = appointments.FirstOrDefault();
            var now = DateTime.Now;

            var viewModel = new HomeDashboardViewModel
            {
                TotalPatients = patients.Count,
                ActiveDoctors = doctors.Count,
                DepartmentCount = departments.Count,
                TodayAppointments = appointments.Count(a => a.ScheduledAt.Date == now.Date),
                PendingRecords = medicalRecords.Count,
                MedicationCount = medications.Count,
                Actions =
                [
                    new DashboardActionItem
                    {
                        Title = "Patients",
                        Description = "Search profiles, register patients, and review medical history.",
                        Controller = "Patients",
                        Accent = "emerald",
                        Metric = $"{patients.Count} registered"
                    },
                    new DashboardActionItem
                    {
                        Title = "Appointments",
                        Description = "Track upcoming visits and manage room scheduling for the day.",
                        Controller = "Appointments",
                        Accent = "sky",
                        Metric = $"{appointments.Count} scheduled"
                    },
                    new DashboardActionItem
                    {
                        Title = "Doctors",
                        Description = "Review coverage, specialties, and department assignments.",
                        Controller = "Doctors",
                        Accent = "violet",
                        Metric = $"{doctors.Count} on roster"
                    },
                    new DashboardActionItem
                    {
                        Title = "Medical Records",
                        Description = "Keep clinical documentation, diagnoses, and prescriptions in sync.",
                        Controller = "MedicalRecords",
                        Accent = "amber",
                        Metric = $"{medicalRecords.Count} active files"
                    }
                ],
                UpcomingAppointments = appointments
                    .Take(4)
                    .Select(a => new DashboardAppointmentItem
                    {
                        TimeLabel = a.ScheduledAt.ToString("ddd, HH:mm"),
                        PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                        DoctorName = $"Dr. {a.Doctor.FirstName} {a.Doctor.LastName}",
                        Room = a.Room,
                        Status = a.Status.ToString()
                    })
                    .ToList(),
                OperationalNotes =
                [
                    new DashboardNoteItem
                    {
                        Title = "Department Coverage",
                        Detail = $"{departments.Count} departments currently covered by {doctors.Count} doctors.",
                        Tone = "calm"
                    },
                    new DashboardNoteItem
                    {
                        Title = "Documentation Queue",
                        Detail = $"{medicalRecords.Count} medical records and {medications.Count} medications are available for review.",
                        Tone = "focus"
                    },
                    new DashboardNoteItem
                    {
                        Title = "Next Priority",
                        Detail = nextAppointment is null
                            ? "No appointments are scheduled right now."
                            : $"Next appointment: {nextAppointment.Patient.FirstName} {nextAppointment.Patient.LastName} with Dr. {nextAppointment.Doctor.LastName} in room {nextAppointment.Room}.",
                        Tone = "alert"
                    }
                ]
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
