using MediatR;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.AccessRequests;

namespace JCA.WorkSpace.Application.Handlers.AccessRequests;

public class ApproveAccessRequestHandler : IRequestHandler<ApproveAccessRequestCommand, bool>
{
    private readonly IAccessRequestRepository _accessRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveAccessRequestHandler(
        IAccessRequestRepository accessRequestRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _accessRequestRepository = accessRequestRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ApproveAccessRequestCommand request, CancellationToken cancellationToken)
    {
        var accessRequest = await _accessRequestRepository.GetByIdAsync(request.RequestId);
        if (accessRequest == null) throw new Exception("Solicitação de acesso não encontrada.");
        if (accessRequest.Status != AccessRequestStatus.Pending) throw new Exception("A solicitação não está pendente.");

        var user = await _userRepository.GetByIdAsync(accessRequest.UserId);
        if (user == null) throw new Exception("Usuário associado à solicitação não encontrado.");

        accessRequest.Status = AccessRequestStatus.Approved;
        accessRequest.UpdatedAt = DateTime.UtcNow;

        user.Profile = accessRequest.RequestedProfile;

        await _accessRequestRepository.UpdateAsync(accessRequest);
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        return true;
    }
}
