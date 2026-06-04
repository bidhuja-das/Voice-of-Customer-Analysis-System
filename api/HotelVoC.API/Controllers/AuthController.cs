using HotelVoC.API.DTOs;
using HotelVoC.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HotelVoC.Core.Services;
using Microsoft.AspNetCore.Authorization;

namespace HotelVoC.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var user = await _authService.ValidateUser(request.Email, request.Password);

        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        var token = _authService.GenerateToken(user);

        return Ok(new LoginResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role
        });
    }


   
}