using HospitalManagementApp.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers.Api;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IMedicalDocumentSummarizer _summarizer;

    public AiController(IMedicalDocumentSummarizer summarizer)
    {
        _summarizer = summarizer;
    }

    [HttpPost("medical-summary")]
    public async Task<ActionResult<MedicalSummaryResult>> SummarizeMedicalDocument(
        [FromBody] MedicalSummaryRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            return Ok(await _summarizer.SummarizeAsync(request.Text, cancellationToken));
        }
        catch (AiConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = ex.Message,
                disclaimer = MedicalSummaryDisclaimer.Text
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(MedicalSummaryRequest.Text), ex.Message);
            return ValidationProblem(ModelState);
        }
    }
}
