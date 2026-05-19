using HotelVoC.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelVoC.API.Controllers;

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
    public async Task<IActionResult> GetComparison()
    {
        var result = await _analyticsService.GetComparison();
        return Ok(result);
    }

    [HttpGet("daily-reports")]
    public async Task<IActionResult> GetDailyReports()
    {
        var result = await _analyticsService.GetDailyReports();
        return Ok(result);
    }
}