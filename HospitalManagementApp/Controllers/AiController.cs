using HospitalManagementApp.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApp.Controllers;

[Authorize]
public class AiController : Controller
{
    private readonly IDataAssistantService _assistant;

    public AiController(IDataAssistantService assistant)
    {
        _assistant = assistant;
    }

    [HttpGet("/ai/data-assistant")]
    public IActionResult DataAssistant()
    {
        return View(new DataAssistantRequest());
    }

    [HttpPost("/ai/data-assistant")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DataAssistant(DataAssistantRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            ViewBag.AssistantResult = await _assistant.AnswerAsync(request.Question, cancellationToken);
        }
        catch (AiConfigurationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(DataAssistantRequest.Question), ex.Message);
        }

        return View(request);
    }
}
