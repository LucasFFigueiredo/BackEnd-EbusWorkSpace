using MediatR;
using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Application.Commands.Spaces;

public class CreateSpaceCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public SpaceType Type { get; set; }
    public int Floor { get; set; }
    public int Capacity { get; set; }
    public string? Sector { get; set; }
    public string? Resources { get; set; }
    public bool RequiresApproval { get; set; }
}