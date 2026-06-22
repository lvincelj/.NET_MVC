namespace HospitalManagementApp.Services.Ai;

public interface IDataAssistantService
{
    Task<DataAssistantResult> AnswerAsync(string question, CancellationToken cancellationToken = default);
}
