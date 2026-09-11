using MediatR;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.Reservations;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class ApproveReservationCommandHandler : IRequestHandler<ApproveReservationCommand, bool>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveReservationCommandHandler(
        IReservationRepository reservationRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ApproveReservationCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsApproved && string.IsNullOrWhiteSpace(request.Justification))
            throw new InvalidOperationException("Uma justificativa é obrigatória ao negar a solicitação.");

        var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId);
        if (reservation == null)
            throw new InvalidOperationException("Reserva não encontrada.");

        bool isExtensionRequest = reservation.Status == ReservationStatus.CheckedIn;
        bool isInitialApproval = reservation.Status == ReservationStatus.AwaitingApproval;

        if (!isInitialApproval && !isExtensionRequest)
            throw new InvalidOperationException($"Esta reserva não está pendente de aprovação. Status atual: {reservation.Status}");

        string action;
        string details;

        if (isInitialApproval)
        {
            reservation.Status = request.IsApproved ? ReservationStatus.Pending : ReservationStatus.Canceled;
            action = request.IsApproved ? "RESERVA_APROVADA" : "RESERVA_NEGADA";
            details = request.IsApproved
                ? "A reserva foi aprovada pela gestão/facilities."
                : $"Reserva negada. Justificativa: {request.Justification}";
        }
        else
        {
            if (request.IsApproved)
            {
                int extraMinutes = 30;
                try
                {
                    var logs = await _auditLogRepository.GetByEntityIdAsync(reservation.Id);

                    var extensionLog = logs.OrderByDescending(l => l.CreatedAt)
                                           .FirstOrDefault(l => l.Action.Contains("EXTENSAO") || l.Action.Contains("EXTENSION"));

                    if (extensionLog != null && !string.IsNullOrEmpty(extensionLog.Details))
                    {
                        using var doc = JsonDocument.Parse(extensionLog.Details);
                        foreach (var property in doc.RootElement.EnumerateObject())
                        {
                            if (property.Name.Equals("AdditionalMinutes", StringComparison.OrdinalIgnoreCase) ||
                                property.Name.Equals("Minutos", StringComparison.OrdinalIgnoreCase))
                            {
                                extraMinutes = property.Value.GetInt32();
                                break;
                            }
                        }

                        if (extraMinutes == 30)
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(extensionLog.Details, @"(\d+)\s*minuto");
                            if (match.Success)
                            {
                                extraMinutes = int.Parse(match.Groups[1].Value);
                            }
                        }
                    }
                }
                catch { }

                reservation.EndTime = reservation.EndTime.AddMinutes(extraMinutes);
                reservation.Status = ReservationStatus.CheckedIn;

                action = "TEMPO_EXTRA_APROVADO";
                details = $"Tempo extra de {extraMinutes} minutos aprovado.";
            }
            else
            {
                reservation.Status = ReservationStatus.CheckedIn;
                action = "TEMPO_EXTRA_NEGADO";
                details = $"Solicitação de tempo extra negada. Justificativa: {request.Justification}";
            }
        }

        await _reservationRepository.UpdateAsync(reservation);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = request.ApproverId,
            Action = action,
            EntityId = reservation.Id,
            Details = JsonSerializer.Serialize(new { Mensagem = details }),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.CommitAsync();

        return true;
    }
}