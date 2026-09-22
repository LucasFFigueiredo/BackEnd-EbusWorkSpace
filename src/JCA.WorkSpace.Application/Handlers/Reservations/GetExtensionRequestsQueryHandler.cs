using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Dtos.Reservations;
using JCA.WorkSpace.Application.Queries.Reservations;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class GetExtensionRequestsQueryHandler : IRequestHandler<GetExtensionRequestsQuery, IEnumerable<ExtensionRequestDto>>
{
    private readonly IExtensionRequestRepository _extensionRequestRepository;

    public GetExtensionRequestsQueryHandler(IExtensionRequestRepository extensionRequestRepository)
    {
        _extensionRequestRepository = extensionRequestRepository;
    }

    public async Task<IEnumerable<ExtensionRequestDto>> Handle(GetExtensionRequestsQuery request, CancellationToken cancellationToken)
    {
        var pendingRequests = await _extensionRequestRepository.GetPendingRequestsAsync();

        return pendingRequests.Select(x => new ExtensionRequestDto
        {
            Id = x.Id,
            ReservationId = x.ReservationId,
            UserName = x.User?.Name ?? "Desconhecido",
            SpaceName = x.Reservation?.Space?.Name ?? "Sala Removida",
            RequestedMinutes = x.RequestedMinutes,
            Justification = x.Justification,
            RequestedAt = x.CreatedAt
        });
    }
}