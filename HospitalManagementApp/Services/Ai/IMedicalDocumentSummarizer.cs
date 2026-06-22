namespace HospitalManagementApp.Services.Ai;

public interface IMedicalDocumentSummarizer
{
    Task<MedicalSummaryResult> SummarizeAsync(string text, CancellationToken cancellationToken = default);
}
