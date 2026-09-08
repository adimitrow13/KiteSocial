using System.Security.Claims;
using KiteSocial.API.DTOs.Auth;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KiteSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (await _userManager.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest(new { Message = "Email is already in use." });
        }

        if (await _userManager.Users.AnyAsync(u => u.UserName == dto.Username))
        {
            return BadRequest(new { Message = "Username is already taken." });
        }

        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Username,
            FullName = dto.FullName,
            SkillLevel = dto.SkillLevel,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        var (token, expiresAt) = _tokenService.CreateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAt,
            User = MapToUserDto(user)
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.Users
            .Include(u => u.HomeSpot)
            .FirstOrDefaultAsync(u => u.UserName == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);

        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid username/email or password." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { Message = "Invalid username/email or password." });
        }

        var (token, expiresAt) = _tokenService.CreateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAt,
            User = MapToUserDto(user)
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.Users
            .Include(u => u.HomeSpot)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        return Ok(MapToUserDto(user));
    }

    private static UserDto MapToUserDto(ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            SkillLevel = user.SkillLevel,
            HomeSpotId = user.HomeSpotId,
            HomeSpotName = user.HomeSpot?.Name,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}
