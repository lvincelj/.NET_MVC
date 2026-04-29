namespace HospitalManagementApp.Models;

public class HomeDashboardViewModel
{
    public int TotalPatients { get; init; }
    public int ActiveDoctors { get; init; }
    public int DepartmentCount { get; init; }
    public int TodayAppointments { get; init; }
    public int PendingRecords { get; init; }
    public int MedicationCount { get; init; }
    public IReadOnlyList<DashboardActionItem> Actions { get; init; } = [];
    public IReadOnlyList<DashboardAppointmentItem> UpcomingAppointments { get; init; } = [];
    public IReadOnlyList<DashboardNoteItem> OperationalNotes { get; init; } = [];
}

public class DashboardActionItem
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Controller { get; init; }
    public required string Accent { get; init; }
    public required string Metric { get; init; }
}

public class DashboardAppointmentItem
{
    public required string TimeLabel { get; init; }
    public required string PatientName { get; init; }
    public required string DoctorName { get; init; }
    public required string Room { get; init; }
    public required string Status { get; init; }
}

public class DashboardNoteItem
{
    public required string Title { get; init; }
    public required string Detail { get; init; }
    public required string Tone { get; init; }
}