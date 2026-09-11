using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid UserId { get; set; }
    public Guid SpaceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime? CheckInAt { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
    public Space? Space { get; set; }
}