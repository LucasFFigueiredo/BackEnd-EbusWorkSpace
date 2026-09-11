using JCA.WorkSpace.Application.Commands.Spaces;
using JCA.WorkSpace.Application.Queries.Spaces;
using JCA.WorkSpace.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JCA.WorkSpace.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class SpacesController : ControllerBase
{
    private readonly IMediator _mediator;
    public record SetResourceBlockDto(bool Blocked, string? Reason, DateTime? BlockedFrom, DateTime? BlockedTo);

    public SpacesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cadastra um novo espaço físico (Mesa ou Sala).
    /// </summary>
    // [Authorize(Roles = "Facilities, Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateSpace([FromBody] CreateSpaceCommand command)
    {
        var spaceId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetSpacesByFloor), new { floor = command.Floor }, new { Id = spaceId });
    }

    /// <summary>
    /// Lista todos os espaços de um andar específico. (Usado para desenhar o mapa)
    /// </summary>
    [HttpGet("floor/{floor}")]
    public async Task<IActionResult> GetSpacesByFloor(int floor)
    {
        var query = new GetSpacesByFloorQuery { Floor = floor };
        var spaces = await _mediator.Send(query);
        return Ok(spaces);
    }

    /// <summary>
    /// Lista os espaços disponíveis com base no período e tipo (Mesa/Sala).
    /// </summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableSpaces([FromQuery] DateTime startTime, [FromQuery] DateTime endTime, [FromQuery] SpaceType type)
    {
        var query = new GetAvailableSpacesQuery
        {
            StartTime = startTime,
            EndTime = endTime,
            Type = type
        };

        var spaces = await _mediator.Send(query);
        return Ok(spaces);
    }

    /// <summary>
    /// Retorna todos os espaços cadastrados no sistema (genérico).
    /// </summary>
    [HttpGet]
    // [Authorize]
    public async Task<IActionResult> GetAllSpaces()
    {
        var query = new GetAllSpacesQuery();
        var spaces = await _mediator.Send(query);
        return Ok(spaces);
    }

    /// <summary>
    /// Retorna apenas os espaços que estão bloqueados / em manutenção.
    /// </summary>
    [HttpGet("maintenance")]
    // [Authorize(Roles = "Admin,Facilities,Manager")]
    public async Task<IActionResult> GetSpacesInMaintenance()
    {
        var query = new GetSpacesInMaintenanceQuery();
        var spaces = await _mediator.Send(query);
        return Ok(spaces);
    }

    /// <summary>
    /// Bloqueia ou desbloqueia um espaço (manutenção).
    /// </summary>
    [HttpPatch("{id}/maintenance")]
    // [Authorize(Roles = "Admin,Facilities")]
    public async Task<IActionResult> SetMaintenance(Guid id, [FromBody] SetSpaceMaintenanceCommand command)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                          User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        // Se o claim existir (produção com JWT), sobrescreve o UserId do body
        if (userIdClaim != null)
            command.UserId = Guid.Parse(userIdClaim.Value);
        else if (command.UserId == Guid.Empty)
            return Unauthorized(new { Message = "ID do usuário não encontrado no token." });

        command.SpaceId = id;

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Edita as características de um espaço existente.
    /// </summary>
    [HttpPut("{id}")]
    // [Authorize(Roles = "Admin,Facilities")]
    public async Task<IActionResult> UpdateSpace(Guid id, [FromBody] UpdateSpaceCommand command)
    {
        command.Id = id;

        var success = await _mediator.Send(command);

        if (!success)
            return NotFound(new { Message = "Espaço não encontrado." });

        return NoContent();
    }

    /// <summary>
    /// Cria múltiplos espaços (Salas ou Mesas) de uma só vez em lote.
    /// </summary>
    [HttpPost("batch")]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBatch([FromBody] IEnumerable<SpaceBatchItem> items)
    {
        var command = new CreateBatchSpaceCommand { Spaces = items };
        var count = await _mediator.Send(command);

        return Ok(new
        {
            success = true,
            result = $"{count} espaços criados com sucesso!"
        });
    }

    /// <summary>
    /// Retorna o mapa de ocupação de um andar para um período específico (Usado para desenhar as mesas cinzas/vermelhas).
    /// </summary>
    [HttpGet("floor/{floor}/occupancy")]
    public async Task<IActionResult> GetFloorOccupancy(int floor, [FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
    {
        var query = new GetFloorOccupancyQuery
        {
            Floor = floor,
            StartTime = startTime,
            EndTime = endTime
        };

        var occupancyMap = await _mediator.Send(query);
        return Ok(occupancyMap);
    }

}