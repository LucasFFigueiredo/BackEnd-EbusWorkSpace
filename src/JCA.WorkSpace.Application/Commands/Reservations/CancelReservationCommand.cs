using MediatR;

namespace JCA.WorkSpace.Application.Commands.Reservations;

public class CancelReservationCommand : IRequest<bool>
{
    public Guid UserId { get; set; } 
    public Guid? ReservationId { get; set; }
    public Guid? BatchId { get; set; }
    public string? UserRole { get; set; }
}