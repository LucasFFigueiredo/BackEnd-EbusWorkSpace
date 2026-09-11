using MediatR;
using Microsoft.Extensions.Logging;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using System.Text.Json;
using JCA.WorkSpace.Application.Commands.Users;

namespace JCA.WorkSpace.Application.Handlers.Users;

public class RequestAccessCommandHandler : IRequestHandler<RequestAccessCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestAccessCommandHandler> _logger;

    public RequestAccessCommandHandler(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ILogger<RequestAccessCommandHandler> logger)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RequestAccessCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null) throw new Exception("Usuário não encontrado.");
        if (user.Profile == request.RequestedProfile) throw new Exception("O usuário já possui este nível de acesso.");

        //_logger.LogWarning("=====================================================");
        //_logger.LogWarning($"[E-MAIL SIMULADO] Para: admin_lucas@jcatlm.com");
        _logger.LogWarning($"[E-MAIL SIMULADO] Assunto: Nova solicitação de acesso");
        //_logger.LogWarning($"[E-MAIL SIMULADO] O usuário {user.Name} ({user.Email}) solicitou a concessão do perfil de {request.RequestedProfile}.");
        //.LogWarning("=====================================================");

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = user.Id,
            Action = "Solicitação de Acesso Enviada",
            EntityId = user.Id,
            Details = JsonSerializer.Serialize(new
            {
                Mensagem = $"Solicitou o perfil: {request.RequestedProfile}. Motivo: {request.Justification}"
            }),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.CommitAsync();
        return true;
    }
}