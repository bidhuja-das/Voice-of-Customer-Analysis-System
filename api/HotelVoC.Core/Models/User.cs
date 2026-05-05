using System.ComponentModel.DataAnnotations;

namespace HotelVoC.Core.Models;

public class User
{
    [Key]
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}