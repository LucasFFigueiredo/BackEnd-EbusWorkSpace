using MediatR;
using JCA.WorkSpace.Domain.Enums;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.Users;

public class RequestAccessCommand : IRequest<bool>
{
    [JsonIgnore]
    public Guid UserId { get; set; }

    public UserProfile RequestedProfile { get; set; }
    public string? Justification { get; set; }
}