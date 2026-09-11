using MediatR;
using JCA.WorkSpace.Application.Dtos.Reservations;

namespace JCA.WorkSpace.Application.Queries.Reservations;

public class GetAllReservationsQuery : IRequest<IEnumerable<ReservationDto>>
{
    public string? Status { get; set; }
}