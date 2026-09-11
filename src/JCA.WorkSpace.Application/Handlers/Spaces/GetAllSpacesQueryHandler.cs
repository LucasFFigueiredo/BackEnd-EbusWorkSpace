using AutoMapper;
using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Dtos.Spaces;
using JCA.WorkSpace.Application.Queries.Spaces;

namespace JCA.WorkSpace.Application.Handlers.Spaces;

public class GetAllSpacesQueryHandler : IRequestHandler<GetAllSpacesQuery, IEnumerable<SpaceDto>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IMapper _mapper;

    public GetAllSpacesQueryHandler(ISpaceRepository spaceRepository, IMapper mapper)
    {
        _spaceRepository = spaceRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SpaceDto>> Handle(GetAllSpacesQuery request, CancellationToken cancellationToken)
    {
        var spaces = await _spaceRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SpaceDto>>(spaces.OrderBy(s => s.Floor).ThenBy(s => s.Name));
    }
}