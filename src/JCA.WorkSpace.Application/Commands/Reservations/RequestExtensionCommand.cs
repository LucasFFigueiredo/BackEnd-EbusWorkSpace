using MediatR;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.Reservations;

public class RequestExtensionCommand : IRequest<bool>
{
    public Guid UserId { get; set; }

    public Guid ReservationId { get; set; }
    public int AdditionalMinutes { get; set; }
    public string Justification { get; set; } = string.Empty;
}