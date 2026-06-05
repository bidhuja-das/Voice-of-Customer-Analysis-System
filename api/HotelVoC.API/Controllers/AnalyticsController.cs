using HotelVoC.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelVoC.API.Controllers;

[Authorize]
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
    public async Task<IActionResult> GetSentimentSummary(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _analyticsService.GetSentimentSummary(from, to);
        return Ok(result);
    }

    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _analyticsService.GetTopics(from, to);
        return Ok(result);
    }

    [HttpGet("comparison")]
    public async Task<IActionResult> GetComparison(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
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