namespace JCA.WorkSpace.Application.Dtos.Spaces;

public class SpaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int Capacity { get; set; }
    public bool IsBlocked { get; set; }
    public string? MaintenanceReason { get; set; }
    public string? Resources { get; set; }
    public bool RequiresApproval { get; set; }
}