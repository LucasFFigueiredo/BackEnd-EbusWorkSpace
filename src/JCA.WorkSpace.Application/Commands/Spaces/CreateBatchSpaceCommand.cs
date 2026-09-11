using MediatR;

namespace JCA.WorkSpace.Application.Commands.Spaces;

public class SpaceBatchItem
{
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; }
    public int Floor { get; set; }
    public int Capacity { get; set; }
    public string? Sector { get; set; }
    public string? Resources { get; set; }
    public bool RequiresApproval { get; set; }
}

public class CreateBatchSpaceCommand : IRequest<int>
{
    public IEnumerable<SpaceBatchItem> Spaces { get; set; } = new List<SpaceBatchItem>();
}