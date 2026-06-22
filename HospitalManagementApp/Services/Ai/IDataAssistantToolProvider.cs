using Microsoft.Extensions.AI;

namespace HospitalManagementApp.Services.Ai;

public interface IDataAssistantToolProvider
{
    IList<AITool> CreateTools();
}
