using MediatR;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.Spaces;

namespace JCA.WorkSpace.Application.Handlers.Spaces;

public class CreateSpaceCommandHandler : IRequestHandler<CreateSpaceCommand, Guid>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSpaceCommandHandler(
        ISpaceRepository spaceRepository, 
        IUnitOfWork unitOfWork)
    {
        _spaceRepository = spaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateSpaceCommand request, CancellationToken cancellationToken)
    {
        var space = new Space
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            Floor = request.Floor,
            Capacity = request.Capacity,
            Sector = request.Sector,
            Resources = request.Resources,
            CreatedAt = DateTime.UtcNow,
            IsBlocked = false,
            RequiresApproval = request.RequiresApproval
        };

        await _spaceRepository.AddAsync(space);
        await _unitOfWork.CommitAsync();

        return space.Id;
    }
}