using JCA.WorkSpace.Application.Commands.Users;
using JCA.WorkSpace.Application.Queries.Users;
using JCA.WorkSpace.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JCA.WorkSpace.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cadastra um novo usuário no sistema.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        try
        {
            var userId = await _mediator.Send(command);
            return Ok(new { Id = userId, Message = "Usuário criado com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Result = ex.Message });
        }
    }

    /// <summary>
    /// Altera o perfil de um usuário (Apenas Admin).
    /// </summary>
    [HttpPut("{id}/role")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleCommand command)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        if (adminIdClaim == null) return Unauthorized(new { Message = "ID do usuário não encontrado no token." });

        command.AdminId = Guid.Parse(adminIdClaim.Value);
        command.TargetUserId = id;

        await _mediator.Send(command);
        return Ok(new { Message = "Perfil atualizado com sucesso." });
    }

    /// <summary>
    /// Envia uma solicitação por e-mail pedindo um novo nível de acesso.
    /// </summary>
    [HttpPost("request-access")]
    //[Authorize]
    public async Task<IActionResult> RequestAccess([FromBody] RequestAccessCommand command)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null) return Unauthorized(new { Message = "ID do usuário não encontrado no token." });

        command.UserId = Guid.Parse(userIdClaim.Value);

        await _mediator.Send(command);
        return Ok(new { Message = "Solicitação enviada com sucesso para os administradores." });
    }

    /// <summary>
    /// Atualiza o setor/departamento do usuário logado (usado no Onboarding).
    /// </summary>
    [HttpPatch("sector")]
    //[Authorize]
    public async Task<IActionResult> UpdateSector([FromBody] UpdateUserSectorCommand command)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null) return Unauthorized(new { Message = "ID do usuário não encontrado no token." });

        command.UserId = Guid.Parse(userIdClaim.Value);

        await _mediator.Send(command);
        return Ok(new { Message = "Setor atualizado com sucesso." });
    }

    /// <summary>
    /// Retorna a lista de todos os usuários do sistema.
    /// </summary>
    [HttpGet]
    // [Authorize(Roles = "Admin,Gestor")]
    public async Task<IActionResult> GetAllUsers()
    {
        var query = new GetUsersQuery();
        var users = await _mediator.Send(query);
        return Ok(users);
    }

    /// <summary>
    /// Lista os colaboradores que solicitaram elevação de acesso.
    /// </summary>
    [HttpGet("requests")]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAccessRequests()
    {
        var query = new GetAccessRequestsQuery();
        var requests = await _mediator.Send(query);
        return Ok(requests);
    }
}