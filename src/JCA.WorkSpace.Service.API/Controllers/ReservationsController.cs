using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Application.Queries.Reservations;
using JCA.WorkSpace.Application.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;


namespace JCA.WorkSpace.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cria uma nova reserva de mesa ou sala.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                          User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        // Em produção, o JWT sobrescreve o UserId do body; em testes, o body é usado como fallback.
        if (userIdClaim != null)
            command.UserId = Guid.Parse(userIdClaim.Value);
        else if (command.UserId == Guid.Empty)
            return Unauthorized("Usuário não autenticado no Token.");

        try
        {
            var reservationId = await _mediator.Send(command);
            return Ok(new { Id = reservationId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Result = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Ocorreu um erro interno ao processar a reserva." });
        }
    }

    /// <summary>
    /// Realiza o check-in através da leitura do QR Code do Espaço.
    /// </summary>
    [HttpPost("scan-checkin")]
    [Authorize]
    public async Task<IActionResult> ScanCheckIn([FromBody] CheckInCommand command)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                          User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (userIdClaim != null)
            command.UserId = Guid.Parse(userIdClaim.Value);
        else if (command.UserId == Guid.Empty)
            return Unauthorized("Usuário não autenticado no Token.");

        await _mediator.Send(command);
        return Ok(new { Message = "Check-in realizado com sucesso." });
    }

    /// <summary>
    /// Cancela uma reserva unitária.
    /// </summary>
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> CancelReservation(Guid id, [FromBody] CancelReservationCommand command)
    {
        if (id != command.ReservationId)
            return BadRequest("O ID da rota difere do ID do corpo da requisição.");

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!string.IsNullOrEmpty(userIdString))
        {
            command.UserId = Guid.Parse(userIdString);
            command.UserRole = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
        }
        else if (command.UserId == Guid.Empty)
        {
            return Unauthorized("Usuário não autenticado no Token.");
        }

        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    

    /// <summary>
    /// Cancela um lote inteiro de reservas recorrentes.
    /// </summary>
    [HttpPatch("batch/{batchId}/cancel")]
    public async Task<IActionResult> CancelBatchReservation(Guid batchId, [FromBody] CancelReservationCommand command)
    {
        if (batchId != command.BatchId)
            return BadRequest("O ID do lote na rota difere do ID do corpo da requisição.");

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                          User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (userIdClaim != null)
        {
            command.UserId = Guid.Parse(userIdClaim.Value);
        }

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Lista o histórico e agendamentos futuros de um usuário.
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserReservations(Guid userId)
    {
        var query = new GetUserReservationsQuery { UserId = userId };

        var reservations = await _mediator.Send(query);
        return Ok(reservations);
    }

    /// <summary>
    /// Retorna os detalhes de uma reserva específica.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetReservationById(Guid id)
    {
        var query = new GetReservationByIdQuery { Id = id };
        var reservation = await _mediator.Send(query);

        if (reservation == null)
            return NotFound(new { Message = "Reserva não encontrada." });

        return Ok(reservation);
    }

    /// <summary>
    /// Cria reservas em lote com algoritmo de resolução de conflitos parciais.
    /// </summary>
    [HttpPost("batch")]
    [Authorize]
    public async Task<IActionResult> CreateBatchReservation([FromBody] CreateBatchReservationCommand command)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                          User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (userIdClaim != null)
            command.UserId = Guid.Parse(userIdClaim.Value);
        else if (command.UserId == Guid.Empty)
            return Unauthorized("Usuário não autenticado no Token.");

        var result = await _mediator.Send(command);

        if (result.SuccessfulCount == 0)
        {
            return Conflict(new
            {
                Message = "Não foi possível realizar as reservas devido a conflitos de horário.",
                result.Errors
            });
        }

        if (result.IsPartial)
        {
            return StatusCode(207, new
            {
                Message = "Algumas reservas foram criadas, mas houve conflitos.",
                Data = result
            });
        }

        return Ok(new
        {
            Message = "Todas as reservas foram criadas com sucesso!",
            Data = result
        });
    }

    /// <summary>
    /// Aprova ou nega uma reserva que exige aprovação prévia.
    /// </summary>
    [HttpPatch("{id}/approval")]
    [Authorize(Roles = "Admin,Facilities")]
    public async Task<IActionResult> ApproveReservation(Guid id, [FromBody] ApproveReservationCommand command)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                          User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (userIdClaim != null)
        {
            command.ApproverId = Guid.Parse(userIdClaim.Value);
        }
        command.ReservationId = id;

        await _mediator.Send(command);

        return Ok(new
        {
            Message = command.IsApproved ? "Reserva aprovada com sucesso!" : "Reserva negada e cancelada com sucesso."
        });
    }

    /// <summary>
    /// Lista todas as reservas. Pode ser filtrado por status.
    /// Ex: /api/reservations?status=AwaitingApproval
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Facilities")]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var query = new GetAllReservationsQuery { Status = status };
        var reservations = await _mediator.Send(query);

        return Ok(reservations);
    }

    /// <summary>
    /// Usuário pede mais tempo na sala (Vai para análise dos Facilities).
    /// </summary>
    [HttpPost("extension/request")]
    public async Task<IActionResult> RequestExtension([FromBody] RequestExtensionCommand command)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                          User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (userIdClaim != null) command.UserId = Guid.Parse(userIdClaim.Value);

        await _mediator.Send(command);
        return Ok(new { Message = "Pedido de extensão enviado aos Facilities." });
    }

    /// <summary>
    /// Facilities aprova a extensão de tempo da sala.
    /// </summary>
    [HttpPatch("extension/approve")]
    [Authorize(Roles = "Admin,Facilities")]
    public async Task<IActionResult> ApproveExtension([FromBody] ApproveExtensionCommand command)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                          User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (userIdClaim != null) command.ApproverId = Guid.Parse(userIdClaim.Value);

        await _mediator.Send(command);
        return Ok(new { Message = "Tempo estendido com sucesso." });
    }

    /// <summary>
    /// Lista todas as solicitações de extensão de tempo pendentes (Visão Facilities).
    /// </summary>
    [HttpGet("extension/requests")]
    [Authorize(Roles = "Admin,Facilities")]
    public async Task<IActionResult> GetExtensionRequests()
    {
        var query = new GetExtensionRequestsQuery();
        var requests = await _mediator.Send(query);

        return Ok(requests);
    }

    /// <summary>
    /// Lista as reservas futuras.
    /// </summary>
    [HttpGet("upcoming")]
    [Authorize(Roles = "Facilities,Admin")]
    public async Task<IActionResult> GetUpcomingReservations([FromQuery] bool onlyRooms = true)
    {
        var query = new GetUpcomingReservationsQuery
        {
            OnlyRooms = onlyRooms
        };

        var reservations = await _mediator.Send(query);

        return Ok(reservations);
    }
}