using JCA.WorkSpace.Domain.Enums;
using System.Text.Json;

namespace JCA.WorkSpace.Domain.Entities;

public class Space
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SpaceType Type { get; set; }
    public int Floor { get; set; }
    public int Capacity { get; set; }
    public string? Sector { get; set; }
    public bool IsBlocked { get; set; }
    public string? MaintenanceReason { get; set; }
    public DateOnly? MaintenanceUntil { get; set; }
    public string? Resources { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool RequiresApproval { get; set; } = false;
}