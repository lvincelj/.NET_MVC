using System.ComponentModel.DataAnnotations;

namespace HospitalManagementApp.Services.Ai;

public sealed class MedicalSummaryRequest
{
    [Required]
    [StringLength(6000, MinimumLength = 20, ErrorMessage = "Text must be between 20 and 6000 characters.")]
    public string Text { get; set; } = string.Empty;
}

public sealed class MedicalSummaryResult
{
    public string Summary { get; set; } = string.Empty;
    public string Disclaimer { get; set; } = MedicalSummaryDisclaimer.Text;
}

public static class MedicalSummaryDisclaimer
{
    public const string Text = "This AI-generated summary is informational only and is not medical advice. Do not send real patient data to external AI services.";
}
