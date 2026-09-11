using MediatR;
using JCA.WorkSpace.Application.Dtos.Spaces;

namespace JCA.WorkSpace.Application.Queries.Spaces;

public class GetFloorOccupancyQuery : IRequest<IEnumerable<SpaceOccupancyDto>>
{
    public int Floor { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}