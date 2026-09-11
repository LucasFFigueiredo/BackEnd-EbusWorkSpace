using MediatR;

namespace JCA.WorkSpace.Application.Commands.Reservations;

public class CreateReservationCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public Guid SpaceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid? BatchId { get; set; }
}