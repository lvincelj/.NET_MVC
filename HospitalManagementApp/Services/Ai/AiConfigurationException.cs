namespace HospitalManagementApp.Services.Ai;

public sealed class AiConfigurationException : InvalidOperationException
{
    public AiConfigurationException(string message) : base(message)
    {
    }
}
