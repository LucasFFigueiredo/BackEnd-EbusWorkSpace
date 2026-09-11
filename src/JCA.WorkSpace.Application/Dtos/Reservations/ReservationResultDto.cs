namespace JCA.WorkSpace.Application.Dtos.Reservations;

public class ReservationResultDto
{
    public Guid BatchId { get; set; }
    public int SuccessfulCount { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public bool IsPartial => SuccessfulCount > 0 && Errors.Any();
    public bool IsSuccess => SuccessfulCount > 0 && !Errors.Any();
}