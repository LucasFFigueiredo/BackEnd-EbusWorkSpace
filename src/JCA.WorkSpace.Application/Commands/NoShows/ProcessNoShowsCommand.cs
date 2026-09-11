using MediatR;

namespace JCA.WorkSpace.Application.Commands.NoShows;

public class ProcessNoShowsCommand : IRequest<bool>
{
    public bool ProcessRooms { get; set; }
    public bool ProcessDesks { get; set; }
}