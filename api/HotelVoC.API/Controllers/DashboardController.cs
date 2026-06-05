using HotelVoC.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace HotelVoC.API.Controllers;

[Authorize(Roles = "Executive")]
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
public async Task<IActionResult> GetStats(
    [FromQuery] DateTime? from = null,
    [FromQuery] DateTime? to = null)
{
    var result = await _analyticsService.GetDashboardStats(from, to);
    return Ok(result);
}
}