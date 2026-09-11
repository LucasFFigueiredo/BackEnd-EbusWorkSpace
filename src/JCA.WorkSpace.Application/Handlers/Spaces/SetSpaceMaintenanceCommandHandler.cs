using System.Text.Json;
using JCA.WorkSpace.Application.Commands.Spaces;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using MediatR;

namespace JCA.WorkSpace.Application.Handlers.Spaces;

public class SetSpaceMaintenanceCommandHandler : IRequestHandler<SetSpaceMaintenanceCommand, bool>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetSpaceMaintenanceCommandHandler(
        ISpaceRepository spaceRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _spaceRepository = spaceRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(SetSpaceMaintenanceCommand request, CancellationToken cancellationToken)
    {
        await _spaceRepository.SetMaintenanceAsync(
            request.SpaceId,
            request.IsBlocked,
            request.MaintenanceReason,
            request.MaintenanceUntil);

        var acao = request.IsBlocked ? "Bloqueio de Espaço (Manutenção)" : "Desbloqueio de Espaço";

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = request.UserId,
            Action = acao,
            EntityId = request.SpaceId,
            Details = JsonSerializer.Serialize(new
            {
                Mensagem = request.MaintenanceReason ?? "Sem justificativa"
            }),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.CommitAsync();

        return true;
    }
}