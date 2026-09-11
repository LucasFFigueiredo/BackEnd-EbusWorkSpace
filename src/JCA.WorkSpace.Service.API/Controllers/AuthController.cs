using MediatR;
using Microsoft.AspNetCore.Mvc;
using JCA.WorkSpace.Application.Commands.Login;

namespace JCA.WorkSpace.Service.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Autentica o usuário via Google OAuth e retorna o token JWT da aplicação.
    /// </summary>
    [HttpPost("google")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] LoginGoogleCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}