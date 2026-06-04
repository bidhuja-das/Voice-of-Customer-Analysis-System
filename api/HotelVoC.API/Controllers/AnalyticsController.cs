using HotelVoC.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HotelVoC.API.Controllers;
[Authorize(Roles = "Analyst,Executive")]
[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;

    public AnalyticsController(AnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("sentiment-summary")]
    public async Task<IActionResult> GetSentimentSummary()
    {
        var result = await _analyticsService.GetSentimentSummary();
        return Ok(result);
    }

    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics()
    {
        var result = await _analyticsService.GetTopics();
        return Ok(result);
    }

    [HttpGet("comparison")]
public async Task<IActionResult> GetComparison(
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to)
{
    var result = await _analyticsService.GetComparison(from, to);
    return Ok(result);
}

    [HttpGet("daily-reports")]
    public async Task<IActionResult> GetDailyReports()
    {
        var result = await _analyticsService.GetDailyReports();
        return Ok(result);
    }
}