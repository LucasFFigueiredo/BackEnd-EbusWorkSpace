using MediatR;
using JCA.WorkSpace.Application.Dtos.Spaces;

namespace JCA.WorkSpace.Application.Queries.Spaces;

public class GetSpacesByFloorQuery : IRequest<IEnumerable<SpaceDto>>
{
    public int Floor { get; set; }
}