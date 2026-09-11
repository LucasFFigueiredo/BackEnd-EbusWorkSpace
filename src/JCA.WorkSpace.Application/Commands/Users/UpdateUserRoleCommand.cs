using MediatR;
using JCA.WorkSpace.Domain.Enums;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.Users;

public class UpdateUserRoleCommand : IRequest<bool>
{
    [JsonIgnore]
    public Guid AdminId { get; set; }

    [JsonIgnore]
    public Guid TargetUserId { get; set; }

    public UserProfile NewProfile { get; set; }
}