using MediatR;
using Microsoft.AspNetCore.Mvc;
using JCA.WorkSpace.Application.Queries.AuditLogs;

namespace JCA.WorkSpace.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Admin")]
public class AuditsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista os logs de auditoria recentes do sistema.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRecentLogs([FromQuery] int limit = 50)
    {
        var query = new GetAuditLogsQuery { Limit = limit };
        var logs = await _mediator.Send(query);
        return Ok(logs);
    }
}