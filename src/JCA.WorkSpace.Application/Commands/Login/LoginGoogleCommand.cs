using MediatR;
using JCA.WorkSpace.Application.Dtos.Auth;

namespace JCA.WorkSpace.Application.Commands.Login;

public class LoginGoogleCommand : IRequest<AuthResponseDto>
{
    public string GoogleToken { get; set; } = string.Empty;
}