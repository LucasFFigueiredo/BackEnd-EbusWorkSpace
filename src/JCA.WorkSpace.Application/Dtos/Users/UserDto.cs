namespace JCA.WorkSpace.Application.Dtos.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public string? Sector { get; set; }
}