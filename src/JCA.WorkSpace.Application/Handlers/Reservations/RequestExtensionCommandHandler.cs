using MediatR;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class RequestExtensionCommandHandler : IRequestHandler<RequestExtensionCommand, bool>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IExtensionRequestRepository _extensionRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestExtensionCommandHandler(
        IReservationRepository reservationRepository, 
        IExtensionRequestRepository extensionRequestRepository, 
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _extensionRequestRepository = extensionRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RequestExtensionCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId);
        if (reservation == null || reservation.UserId != request.UserId)
            throw new InvalidOperationException("Reserva não encontrada ou você não tem permissão.");

        if (await _extensionRequestRepository.HasPendingRequestAsync(request.ReservationId))
            throw new InvalidOperationException("Já existe uma solicitação de extensão pendente para esta reserva.");

        var extensionRequest = new ExtensionRequest
        {
            Id = Guid.NewGuid(),
            ReservationId = reservation.Id,
            UserId = request.UserId,
            RequestedMinutes = request.AdditionalMinutes,
            Justification = request.Justification,
            Status = ExtensionRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _extensionRequestRepository.AddAsync(extensionRequest);

        await _unitOfWork.CommitAsync();
        return true;
    }
}