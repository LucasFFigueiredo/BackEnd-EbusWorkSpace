using MediatR;
using JCA.WorkSpace.Application.Dtos.Spaces;

namespace JCA.WorkSpace.Application.Queries.Spaces;

public class GetAllSpacesQuery : IRequest<IEnumerable<SpaceDto>>
{
}