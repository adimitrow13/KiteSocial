using KiteSocial.API.DTOs.Sessions;

namespace KiteSocial.API.Services.Interfaces;

public enum SessionDeleteResult
{
    Success,
    NotFound,
    Forbidden
}

public interface ISessionService
{
    Task<IEnumerable<KiteSessionDto>> GetSessionsAsync(Guid? spotId, Guid? userId);
    Task<KiteSessionDto?> GetSessionByIdAsync(Guid id);
    Task<KiteSessionDto?> CreateSessionAsync(CreateKiteSessionDto dto, Guid userId);
    Task<IEnumerable<KiteSessionDto>> GetUserSessionsAsync(Guid userId);
    Task<UserSessionStatsDto> GetUserStatsAsync(Guid userId);
    Task<SessionDeleteResult> DeleteSessionAsync(Guid id, Guid userId);
}
