using Microsoft.AspNetCore.Mvc;

namespace Privatekonomi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoalsController : ControllerBase
{
    private readonly ILogger<GoalsController> _logger;

    public GoalsController(ILogger<GoalsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lista sparmål (Placeholder - ingen implementation än)
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<object>> GetGoals()
    {
        _logger.LogWarning("Goals endpoint called but not yet implemented");
        return Ok(new List<object>());
    }

    /// <summary>
    /// Skapa sparmål (Placeholder - ingen implementation än)
    /// </summary>
    [HttpPost]
    public ActionResult<object> CreateGoal([FromBody] object goal)
    {
        _logger.LogWarning("Create goal endpoint called but not yet implemented");
        return StatusCode(501, new { message = "Goals feature not yet implemented" });
    }
}
