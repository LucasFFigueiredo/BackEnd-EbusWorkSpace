using MediatR;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.Reservations;

public class CheckInCommand : IRequest<bool>
{
    public Guid UserId { get; set; }

    public Guid SpaceId { get; set; }
    public double UserLatitude { get; set; }
    public double UserLongitude { get; set; }
}