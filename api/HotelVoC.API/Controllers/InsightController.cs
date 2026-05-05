using HotelVoC.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelVoC.API.Controllers;

[ApiController]
[Route("api/insights")]
public class InsightController : ControllerBase
{
    private readonly InsightService _insightService;

    public InsightController(InsightService insightService)
    {
        _insightService = insightService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate()
    {
        await _insightService.GenerateInsights();
        return Ok(new { message = "Insights generated successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var insights = await _insightService.GetAll();
        return Ok(insights);
    }
}