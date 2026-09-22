using MediatR;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.AccessRequests;

namespace JCA.WorkSpace.Application.Handlers.AccessRequests;

public class CreateAccessRequestHandler : IRequestHandler<CreateAccessRequestCommand, Guid>
{
    private readonly IAccessRequestRepository _accessRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccessRequestHandler(
        IAccessRequestRepository accessRequestRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _accessRequestRepository = accessRequestRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAccessRequestCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null) throw new Exception("Usuário não encontrado.");
        if (user.Profile == request.RequestedProfile) throw new Exception("O usuário já possui este nível de acesso.");

        var hasPending = await _accessRequestRepository.HasPendingRequestAsync(request.UserId);
        if (hasPending) throw new Exception("Já existe uma solicitação de acesso pendente para este usuário.");

        var accessRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RequestedProfile = request.RequestedProfile,
            Status = AccessRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _accessRequestRepository.AddAsync(accessRequest);
        await _unitOfWork.CommitAsync();

        return accessRequest.Id;
    }
}
