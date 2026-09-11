using MediatR;
using JCA.WorkSpace.Domain.Enums;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.Spaces;

public class UpdateSpaceCommand : IRequest<bool>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public SpaceType Type { get; set; }
    public int Floor { get; set; }
    public int Capacity { get; set; }
    public string? Sector { get; set; }
    public string? Resources { get; set; }
    public bool RequiresApproval { get; set; }
}