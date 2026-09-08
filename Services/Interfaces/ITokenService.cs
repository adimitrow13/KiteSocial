using KiteSocial.API.Models.Entities;

namespace KiteSocial.API.Services.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user);
}
