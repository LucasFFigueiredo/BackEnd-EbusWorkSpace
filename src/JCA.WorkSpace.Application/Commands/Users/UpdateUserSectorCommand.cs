using MediatR;
using System.Text.Json.Serialization;

namespace JCA.WorkSpace.Application.Commands.Users;

public class UpdateUserSectorCommand : IRequest<bool>
{
    [JsonIgnore]
    public Guid UserId { get; set; }

    public string Sector { get; set; } = string.Empty;
}