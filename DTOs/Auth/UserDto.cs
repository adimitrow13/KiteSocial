using KiteSocial.API.Models.Entities;

namespace KiteSocial.API.DTOs.Auth;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public SkillLevel SkillLevel { get; set; }
    public Guid? HomeSpotId { get; set; }
    public string? HomeSpotName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
