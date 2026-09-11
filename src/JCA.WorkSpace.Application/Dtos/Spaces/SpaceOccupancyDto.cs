namespace JCA.WorkSpace.Application.Dtos.Spaces;

public class SpaceOccupancyDto
{
    public Guid SpaceId { get; set; }
    public string Name { get; set; } = string.Empty;

    public bool IsBlocked { get; set; }
    public string? MaintenanceReason { get; set; }

    public bool IsOccupied { get; set; }
    public string? OccupantName { get; set; }
    public Guid? OccupantId { get; set; }
}