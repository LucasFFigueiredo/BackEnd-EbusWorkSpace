using System.Text.Json;
using MediatR;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.Users;

namespace JCA.WorkSpace.Application.Handlers.Users;

public class UpdateUserSectorCommandHandler : IRequestHandler<UpdateUserSectorCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserSectorCommandHandler(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateUserSectorCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null) throw new Exception("Usuário não encontrado.");

        var oldSector = user.Sector;
        user.Sector = request.Sector;

        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = user.Id,
            Action = "Atualização de Setor (Onboarding)",
            EntityId = user.Id,
            Details = JsonSerializer.Serialize(new
            {
                Mensagem = $"Setor atualizado de '{oldSector ?? "Nenhum"}' para '{request.Sector}'"
            }),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.CommitAsync();
        return true;
    }
}