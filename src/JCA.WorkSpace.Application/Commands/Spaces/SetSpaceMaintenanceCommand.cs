using MediatR;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.Spaces;

public class SetSpaceMaintenanceCommand : IRequest<bool>
{
    [JsonIgnore]
    public Guid SpaceId { get; set; }

    public Guid UserId { get; set; }

    public bool IsBlocked { get; set; }
    public string? MaintenanceReason { get; set; }
    public DateOnly? MaintenanceUntil { get; set; }
}