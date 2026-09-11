using MediatR;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.Spaces;

namespace JCA.WorkSpace.Application.Handlers.Spaces;

public class UpdateSpaceCommandHandler : IRequestHandler<UpdateSpaceCommand, bool>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSpaceCommandHandler(ISpaceRepository spaceRepository, IUnitOfWork unitOfWork)
    {
        _spaceRepository = spaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateSpaceCommand request, CancellationToken cancellationToken)
    {
        var space = await _spaceRepository.GetByIdAsync(request.Id);

        if (space == null)
            return false;

        space.Name = request.Name;
        space.Type = request.Type;
        space.Floor = request.Floor;
        space.Capacity = request.Capacity;
        space.Sector = request.Sector;
        space.Resources = request.Resources;
        space.RequiresApproval = request.RequiresApproval;

        await _spaceRepository.UpdateAsync(space);
        await _unitOfWork.CommitAsync();

        return true;
    }
}