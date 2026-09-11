using AutoMapper;
using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Dtos.Reservations;
using JCA.WorkSpace.Application.Queries.Users;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class GetUserReservationsQueryHandler : IRequestHandler<GetUserReservationsQuery, IEnumerable<ReservationDto>>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IMapper _mapper;

    public GetUserReservationsQueryHandler(
        IReservationRepository reservationRepository, 
        IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ReservationDto>> Handle(GetUserReservationsQuery request, CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.GetByUserIdAsync(request.UserId);
        return _mapper.Map<IEnumerable<ReservationDto>>(reservations);
    }
}