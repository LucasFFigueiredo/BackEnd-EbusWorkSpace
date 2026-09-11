using MediatR;
using JCA.WorkSpace.Application.Dtos.Reservations;

namespace JCA.WorkSpace.Application.Queries.Users;

public class GetUserReservationsQuery : IRequest<IEnumerable<ReservationDto>>
{
    public Guid UserId { get; set; }
}