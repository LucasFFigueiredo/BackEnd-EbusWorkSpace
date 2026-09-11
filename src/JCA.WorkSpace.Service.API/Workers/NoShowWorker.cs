using JCA.WorkSpace.Application.Commands.NoShows;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JCA.WorkSpace.Service.API.Workers;

public class NoShowWorker : BackgroundService
{
    private readonly ILogger<NoShowWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private DateTime _lastDeskProcessDate = DateTime.MinValue;

    public NoShowWorker(ILogger<NoShowWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Motor de No-Show iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                bool shouldProcessDesks = CheckIfShouldProcessDesks();

                var command = new ProcessNoShowsCommand
                {
                    ProcessRooms = true,
                    ProcessDesks = shouldProcessDesks
                };

                await mediator.Send(command, stoppingToken);

                var spaceRepo = scope.ServiceProvider.GetRequiredService<ISpaceRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var auditRepo = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
                var reservationRepo = scope.ServiceProvider.GetRequiredService<IReservationRepository>();

                await reservationRepo.CompleteExpiredReservationsAsync(DateTime.UtcNow);
                await unitOfWork.CommitAsync();

                var today = DateOnly.FromDateTime(GetCurrentBrazilTime());
                var blockedSpaces = await spaceRepo.GetInMaintenanceAsync();

                foreach (var space in blockedSpaces)
                {
                    if (space.MaintenanceUntil.HasValue && space.MaintenanceUntil.Value < today)
                    {
                        await spaceRepo.SetMaintenanceAsync(space.Id, false, null, null);

                        await auditRepo.AddAsync(new Domain.Entities.AuditLog
                        {
                            Action = "DESBLOQUEIO_AUTOMATICO",
                            EntityId = space.Id,
                            Details = "{\"Mensagem\": \"A sala foi desbloqueada automaticamente pelo sistema (Data de manutenção expirou).\"}",
                            CreatedAt = DateTime.UtcNow
                        });

                        await unitOfWork.CommitAsync();
                        _logger.LogInformation($"[WORKER] O espaço {space.Name} foi desbloqueado automaticamente.");
                    }
                }

                if (shouldProcessDesks)
                {
                    _lastDeskProcessDate = GetCurrentBrazilTime().Date;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro grave na execução do Worker de No-Show.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private bool CheckIfShouldProcessDesks() 
    {
        var nowInBrazil = GetCurrentBrazilTime();

        if (nowInBrazil.TimeOfDay >= new TimeSpan(10, 31, 0) && _lastDeskProcessDate.Date != nowInBrazil.Date)
        {
            return true;
        }
        return false;
    }

    private DateTime GetCurrentBrazilTime()
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }
        catch (TimeZoneNotFoundException)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }
    }
}