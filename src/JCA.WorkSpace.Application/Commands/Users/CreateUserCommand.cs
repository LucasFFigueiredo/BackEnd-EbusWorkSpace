using MediatR;
using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Application.Commands.Users;

public class CreateUserCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Sector { get; set; }
    public UserProfile Profile { get; set; }
}