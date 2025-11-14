using Microsoft.AspNetCore.Mvc;

namespace Project.API.Controllers;

[ApiController]

public class HealthCheckController : ControllerBase
{
    [HttpGet("HealthCheck")]
    public IActionResult Get() => Ok("Healthy");
}