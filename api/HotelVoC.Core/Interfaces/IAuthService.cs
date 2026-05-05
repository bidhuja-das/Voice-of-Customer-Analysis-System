using HotelVoC.Core.Models;

namespace HotelVoC.Core.Interfaces;

public interface IAuthService
{
    Task<User?> ValidateUser(string email, string password);
    string GenerateToken(User user);
}