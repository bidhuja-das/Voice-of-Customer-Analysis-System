using HotelVoC.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelVoC.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;

    public DashboardController(AnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _analyticsService.GetDashboardStats();
        return Ok(result);
    }
}