namespace JCA.WorkSpace.Application.Dtos.Reservations;

public class ExtensionRequestDto
{
    public Guid ReservationId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string SpaceName { get; set; } = string.Empty;
    public int RequestedMinutes { get; set; }
    public string Justification { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}