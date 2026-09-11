using AutoMapper;
using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Dtos.Spaces;
using JCA.WorkSpace.Application.Queries.Spaces;

namespace JCA.WorkSpace.Application.Handlers.Spaces;

public class GetAvailableSpacesQueryHandler : IRequestHandler<GetAvailableSpacesQuery, IEnumerable<SpaceDto>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IMapper _mapper;

    public GetAvailableSpacesQueryHandler(
        ISpaceRepository spaceRepository, 
        IMapper mapper)
    {
        _spaceRepository = spaceRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SpaceDto>> Handle(GetAvailableSpacesQuery request, CancellationToken cancellationToken)
    {
        var spaces = await _spaceRepository.GetAvailableAsync(request.StartTime, request.EndTime, request.Type);

        return _mapper.Map<IEnumerable<SpaceDto>>(spaces);
    }
}