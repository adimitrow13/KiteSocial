using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace KiteSocial.API.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config)
    {
        _config = config;
        var jwtKey = _config["Jwt:Key"] 
            ?? throw new InvalidOperationException("Jwt:Key is not configured in appsettings.");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    }

    public (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("FullName", user.FullName)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

        var expireDays = int.TryParse(_config["Jwt:ExpireDays"], out var days) ? days : 7;
        var expires = DateTime.UtcNow.AddDays(expireDays);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = creds,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expires);
    }
}
