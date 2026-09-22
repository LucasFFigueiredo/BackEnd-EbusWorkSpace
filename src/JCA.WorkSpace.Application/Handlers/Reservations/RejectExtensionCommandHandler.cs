using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class RejectExtensionCommandHandler : IRequestHandler<RejectExtensionCommand, bool>
{
    private readonly IExtensionRequestRepository _extensionRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectExtensionCommandHandler(
        IExtensionRequestRepository extensionRequestRepository, 
        IUnitOfWork unitOfWork)
    {
        _extensionRequestRepository = extensionRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RejectExtensionCommand request, CancellationToken cancellationToken)
    {
        var extensionRequest = await _extensionRequestRepository.GetByIdAsync(request.ExtensionRequestId);
        if (extensionRequest == null) throw new InvalidOperationException("Solicitação de extensão não encontrada.");
        if (extensionRequest.Status != ExtensionRequestStatus.Pending) throw new InvalidOperationException("A solicitação não está pendente.");

        extensionRequest.Status = ExtensionRequestStatus.Rejected;
        extensionRequest.UpdatedAt = DateTime.UtcNow;

        await _extensionRequestRepository.UpdateAsync(extensionRequest);
        await _unitOfWork.CommitAsync();

        return true;
    }
}
