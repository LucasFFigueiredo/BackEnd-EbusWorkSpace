using MediatR;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.AccessRequests;

namespace JCA.WorkSpace.Application.Handlers.AccessRequests;

public class RejectAccessRequestHandler : IRequestHandler<RejectAccessRequestCommand, bool>
{
    private readonly IAccessRequestRepository _accessRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectAccessRequestHandler(
        IAccessRequestRepository accessRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _accessRequestRepository = accessRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RejectAccessRequestCommand request, CancellationToken cancellationToken)
    {
        var accessRequest = await _accessRequestRepository.GetByIdAsync(request.RequestId);
        if (accessRequest == null) throw new Exception("Solicitação de acesso não encontrada.");
        if (accessRequest.Status != AccessRequestStatus.Pending) throw new Exception("A solicitação não está pendente.");

        accessRequest.Status = AccessRequestStatus.Rejected;
        accessRequest.UpdatedAt = DateTime.UtcNow;

        await _accessRequestRepository.UpdateAsync(accessRequest);
        await _unitOfWork.CommitAsync();

        return true;
    }
}
