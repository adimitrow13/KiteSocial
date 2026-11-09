using KiteSocial.API.DTOs.Sessions;
using KiteSocial.API.DTOs.Spots;
using KiteSocial.API.Models.Entities;

namespace KiteSocial.API.Services.Interfaces;

public interface ISpotService
{
    Task<IEnumerable<SpotDto>> GetAllSpotsAsync(string? search, string? country, WaterType? waterType);
    Task<SpotDto?> GetSpotByIdAsync(Guid id);
    Task<SpotDto> CreateSpotAsync(CreateSpotDto dto, Guid? userId);
    Task<IEnumerable<KiteSessionDto>?> GetSpotSessionsAsync(Guid spotId);
}
