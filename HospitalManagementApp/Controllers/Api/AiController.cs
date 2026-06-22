using HospitalManagementApp.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers.Api;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IDataAssistantService _assistant;

    public AiController(IDataAssistantService assistant)
    {
        _assistant = assistant;
    }

    [HttpPost("data-assistant")]
    public async Task<ActionResult<DataAssistantResult>> AskDataAssistant(
        [FromBody] DataAssistantRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            return Ok(await _assistant.AnswerAsync(request.Question, cancellationToken));
        }
        catch (AiConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = ex.Message,
                disclaimer = DataAssistantDisclaimer.Text
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(DataAssistantRequest.Question), ex.Message);
            return ValidationProblem(ModelState);
        }
    }
}
