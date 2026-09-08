using Microsoft.AspNetCore.Mvc;

namespace KiteSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "KiteSocial.API",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            ServerTimeUtc = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }
}
