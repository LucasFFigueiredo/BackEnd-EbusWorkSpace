using MediatR;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using System.Text.Json;
using JCA.WorkSpace.Application.Commands.Users;

namespace JCA.WorkSpace.Application.Handlers.Users;

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserRoleCommandHandler(IUserRepository userRepository, IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.TargetUserId);
        if (user == null) throw new Exception("Usuário não encontrado.");

        var oldProfile = user.Profile;
        user.Profile = request.NewProfile;

        await _userRepository.UpdateAsync(user);

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = request.AdminId,
            Action = "Alteração de Perfil de Acesso",
            EntityId = user.Id,
            Details = JsonSerializer.Serialize(new { Mensagem = $"Alterou o perfil de '{oldProfile}' para '{request.NewProfile}'" }),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.CommitAsync();
        return true;
    }
}