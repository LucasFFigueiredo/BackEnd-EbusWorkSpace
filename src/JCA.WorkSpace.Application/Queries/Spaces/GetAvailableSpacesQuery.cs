using MediatR;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Application.Dtos.Spaces;

namespace JCA.WorkSpace.Application.Queries.Spaces;

public class GetAvailableSpacesQuery : IRequest<IEnumerable<SpaceDto>>
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public SpaceType Type { get; set; }
}