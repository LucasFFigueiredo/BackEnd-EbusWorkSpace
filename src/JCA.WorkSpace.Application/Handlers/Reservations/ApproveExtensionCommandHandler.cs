using MediatR;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Domain.Entities;
using System.Text.Json;
using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class ApproveExtensionCommandHandler : IRequestHandler<ApproveExtensionCommand, bool>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IExtensionRequestRepository _extensionRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveExtensionCommandHandler(
        IReservationRepository reservationRepository, 
        IExtensionRequestRepository extensionRequestRepository, 
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _extensionRequestRepository = extensionRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ApproveExtensionCommand request, CancellationToken cancellationToken)
    {
        var extensionRequest = await _extensionRequestRepository.GetByIdAsync(request.ExtensionRequestId);
        if (extensionRequest == null) throw new InvalidOperationException("Solicitação de extensão não encontrada.");
        if (extensionRequest.Status != ExtensionRequestStatus.Pending) throw new InvalidOperationException("A solicitação não está pendente.");

        var reservation = await _reservationRepository.GetByIdAsync(extensionRequest.ReservationId);
        if (reservation == null) throw new InvalidOperationException("Reserva não encontrada.");

        var proposedEndTime = reservation.EndTime.AddMinutes(extensionRequest.RequestedMinutes);
        var conflicts = await _reservationRepository.GetConflictsAsync(reservation.SpaceId, reservation.EndTime, proposedEndTime);

        if (conflicts.Any()) throw new InvalidOperationException("Não é possível aprovar a extensão. A sala já foi reservada logo em seguida.");

        // Atualizar Reserva
        reservation.EndTime = proposedEndTime;
        await _reservationRepository.UpdateAsync(reservation);

        // Atualizar Solicitação
        extensionRequest.Status = ExtensionRequestStatus.Approved;
        extensionRequest.UpdatedAt = DateTime.UtcNow;
        await _extensionRequestRepository.UpdateAsync(extensionRequest);

        await _unitOfWork.CommitAsync();
        return true;
    }
}