using System.ComponentModel.DataAnnotations;
using KiteSocial.API.Models.Entities;

namespace KiteSocial.API.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    [MaxLength(30)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    public SkillLevel SkillLevel { get; set; } = SkillLevel.Beginner;
}
