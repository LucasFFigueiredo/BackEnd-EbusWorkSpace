using MediatR;
using Microsoft.AspNetCore.Mvc;
using JCA.WorkSpace.Application.Queries.Dashboards;

namespace JCA.WorkSpace.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Gestor, Admin")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retorna as métricas de ocupação e efetividade do sistema.
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var query = new GetDashboardMetricsQuery { StartDate = startDate, EndDate = endDate };
        var metrics = await _mediator.Send(query);
        return Ok(metrics);
    }

    /// <summary>
    /// Retorna as métricas e agregações gerais para o Dashboard.
    /// </summary>
    [HttpGet("general")]
    // [Authorize(Roles = "Admin,Manager,Facilities")]
    public async Task<IActionResult> GetGeneralMetrics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var query = new GetDashboardMetricsQuery { StartDate = start, EndDate = end };
        var metrics = await _mediator.Send(query);

        return Ok(metrics);
    }

    /// <summary>
    /// Retorna as métricas individuais de um colaborador específico (Visão Gestor/Admin).
    /// </summary>
    [HttpGet("user/{userId}")]
    // [Authorize(Roles = "Admin,Manager,Facilities")]
    public async Task<IActionResult> GetUserMetrics(Guid userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var query = new GetUserDashboardQuery { UserId = userId, StartDate = start, EndDate = end };
        var metrics = await _mediator.Send(query);

        return Ok(metrics);
    }

    /// <summary>
    /// Retorna as métricas individuais do próprio usuário logado.
    /// </summary>
    [HttpGet("me")]
    // [Authorize]
    public async Task<IActionResult> GetMyMetrics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                          User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null)
            return Unauthorized(new { Message = "ID do usuário não encontrado no token." });

        var userId = Guid.Parse(userIdClaim.Value);

        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var query = new GetUserDashboardQuery { UserId = userId, StartDate = start, EndDate = end };
        var metrics = await _mediator.Send(query);

        return Ok(metrics);
    }
}