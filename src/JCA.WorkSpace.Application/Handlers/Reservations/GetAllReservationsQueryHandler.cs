using MediatR;
using AutoMapper;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Application.Dtos.Reservations;
using JCA.WorkSpace.Application.Queries.Reservations;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class GetAllReservationsQueryHandler : IRequestHandler<GetAllReservationsQuery, IEnumerable<ReservationDto>>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IMapper _mapper;

    public GetAllReservationsQueryHandler(IReservationRepository reservationRepository, IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ReservationDto>> Handle(GetAllReservationsQuery request, CancellationToken cancellationToken)
    {
        ReservationStatus? filterStatus = null;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<ReservationStatus>(request.Status, true, out var parsedStatus))
            {
                filterStatus = parsedStatus;
            }
            else
            {
                throw new InvalidOperationException($"O status informado ('{request.Status}') não é válido.");
            }
        }

        var reservations = await _reservationRepository.GetAllWithDetailsAsync(filterStatus);

        return _mapper.Map<IEnumerable<ReservationDto>>(reservations);
    }
}