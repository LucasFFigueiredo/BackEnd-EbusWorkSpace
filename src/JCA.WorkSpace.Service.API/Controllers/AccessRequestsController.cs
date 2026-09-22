using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JCA.WorkSpace.Application.Commands.AccessRequests;
using JCA.WorkSpace.Application.Queries.Users;

namespace JCA.WorkSpace.Service.API.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize]
public class AccessRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccessRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccessRequestCommand command)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null) return Unauthorized(new { Message = "ID do usuário não encontrado no token." });

            command.UserId = Guid.Parse(userIdClaim.Value);

            var result = await _mediator.Send(command);
            return Ok(new { success = true, id = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, result = ex.Message });
        }
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _mediator.Send(new GetAccessRequestsQuery());
        return Ok(result);
    }

    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveAccessRequestCommand { RequestId = id });
        return Ok(new { success = result });
    }

    [HttpPatch("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await _mediator.Send(new RejectAccessRequestCommand { RequestId = id });
        return Ok(new { success = result });
    }
}
