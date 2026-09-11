using JCA.WorkSpace.Application.Dtos.Users;

namespace JCA.WorkSpace.Application.Dtos.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}