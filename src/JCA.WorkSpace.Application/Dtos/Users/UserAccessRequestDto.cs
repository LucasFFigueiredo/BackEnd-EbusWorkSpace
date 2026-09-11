namespace JCA.WorkSpace.Application.Dtos.Users;

public class UserAccessRequestDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string CurrentProfile { get; set; } = string.Empty;
    public string RequestedProfile { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}