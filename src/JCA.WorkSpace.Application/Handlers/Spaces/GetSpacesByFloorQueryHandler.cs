using AutoMapper;
using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Dtos.Spaces;
using JCA.WorkSpace.Application.Queries.Spaces;

namespace JCA.WorkSpace.Application.Handlers.Spaces;

public class GetSpacesByFloorQueryHandler : IRequestHandler<GetSpacesByFloorQuery, IEnumerable<SpaceDto>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IMapper _mapper;

    public GetSpacesByFloorQueryHandler(
        ISpaceRepository spaceRepository, 
        IMapper mapper)
    {
        _spaceRepository = spaceRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SpaceDto>> Handle(GetSpacesByFloorQuery request, CancellationToken cancellationToken)
    {
        var spaces = await _spaceRepository.GetByFloorAsync(request.Floor);
        return _mapper.Map<IEnumerable<SpaceDto>>(spaces);
    }
}