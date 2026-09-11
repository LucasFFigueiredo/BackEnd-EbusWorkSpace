using MediatR;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.Spaces;

namespace JCA.WorkSpace.Application.Handlers.Spaces;

public class CreateBatchSpaceCommandHandler : IRequestHandler<CreateBatchSpaceCommand, int>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBatchSpaceCommandHandler(ISpaceRepository spaceRepository, IUnitOfWork unitOfWork)
    {
        _spaceRepository = spaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateBatchSpaceCommand request, CancellationToken cancellationToken)
    {
        int count = 0;

        foreach (var item in request.Spaces)
        {
            var space = new Space
            {
                Id = Guid.NewGuid(),
                Name = item.Name,
                Type = (SpaceType)item.Type,
                Floor = item.Floor,
                Capacity = item.Capacity,
                Sector = item.Sector,
                Resources = item.Resources,
                RequiresApproval = item.RequiresApproval,
                CreatedAt = DateTime.UtcNow
            };

            await _spaceRepository.AddAsync(space);
            count++;
        }

        await _unitOfWork.CommitAsync();

        return count;
    }
}