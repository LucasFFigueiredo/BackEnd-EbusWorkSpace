using MediatR;
using JCA.WorkSpace.Application.Dtos.Reservations;

namespace JCA.WorkSpace.Application.Queries.Reservations;

public class GetExtensionRequestsQuery : IRequest<IEnumerable<ExtensionRequestDto>> { }