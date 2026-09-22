using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Domain.Entities;

public class AccessRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public UserProfile RequestedProfile { get; set; }
    public AccessRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
