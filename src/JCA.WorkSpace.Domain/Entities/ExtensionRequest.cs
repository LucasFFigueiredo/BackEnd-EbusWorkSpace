using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Domain.Entities;

public class ExtensionRequest
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public Guid UserId { get; set; }
    public int RequestedMinutes { get; set; }
    public string Justification { get; set; } = string.Empty;
    public ExtensionRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Reservation? Reservation { get; set; }
    public User? User { get; set; }
}
