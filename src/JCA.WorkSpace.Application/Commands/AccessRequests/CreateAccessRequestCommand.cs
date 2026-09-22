using MediatR;
using JCA.WorkSpace.Domain.Enums;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.AccessRequests;

public class CreateAccessRequestCommand : IRequest<Guid>
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public UserProfile RequestedProfile { get; set; }
}
