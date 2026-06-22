using HospitalManagementApp.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

[Authorize]
public class AiController : Controller
{
    private readonly IMedicalDocumentSummarizer _summarizer;

    public AiController(IMedicalDocumentSummarizer summarizer)
    {
        _summarizer = summarizer;
    }

    [HttpGet("/ai/medical-summary")]
    public IActionResult MedicalSummary()
    {
        return View(new MedicalSummaryRequest());
    }

    [HttpPost("/ai/medical-summary")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MedicalSummary(MedicalSummaryRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            ViewBag.SummaryResult = await _summarizer.SummarizeAsync(request.Text, cancellationToken);
        }
        catch (AiConfigurationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(MedicalSummaryRequest.Text), ex.Message);
        }

        return View(request);
    }
}
