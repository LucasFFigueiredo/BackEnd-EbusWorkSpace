using MediatR;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using JCA.WorkSpace.Application.Commands.Reservations;

namespace JCA.WorkSpace.Application.Handlers.Reservations;

public class CheckInCommandHandler : IRequestHandler<CheckInCommand, bool>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    // Coordenadas da Matriz Ebus
    private const double OdpLatitude = -23.503187;
    private const double OdpLongitude = -46.848757;
    private const double MaxDistanceMeters = 500.0;

    public CheckInCommandHandler(
        IReservationRepository reservationRepository,
        ISpaceRepository spaceRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _spaceRepository = spaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var space = await _spaceRepository.GetByIdAsync(request.SpaceId);
        if (space == null) throw new InvalidOperationException("QR Code inválido. Este espaço não existe no sistema.");

        string tipoEspaco = space.Type == SpaceType.Room ? "sala" : "mesa";

        // 1. VALIDAÇÃO DE DISTÂNCIA / GEOLOCALIZAÇÃO
        double distance = CalculateHaversineDistance(OdpLatitude, OdpLongitude, request.UserLatitude, request.UserLongitude);
        if (distance > MaxDistanceMeters)
        {
            throw new InvalidOperationException($"Você está muito longe do escritório. Distância atual: {Math.Round(distance)}m. Aproxime-se para fazer o check-in.");
        }

        var nowUtc = DateTime.UtcNow;
        var currentReservation = await _reservationRepository.GetCurrentBySpaceIdAsync(request.SpaceId, nowUtc);

        if (currentReservation == null)
        {
            if (space.Type == SpaceType.Room)
                throw new InvalidOperationException("FREE_ROOM|Esta sala está livre no momento. Por favor, retorne ao menu e faça a reserva informando o período de uso.");
            else
                throw new InvalidOperationException("FREE_DESK|Esta mesa está livre no momento. Deseja reservá-la pelo resto do dia e fazer o check-in automático?");
        }

        if (currentReservation.UserId != request.UserId)
        {
            var occupantName = currentReservation.User?.Name ?? "outro colaborador";
            throw new InvalidOperationException($"A sala '{space.Name}' já está reservada para {occupantName} neste momento.");
        }

        if (currentReservation.Status == ReservationStatus.CheckedIn)
        {
            throw new InvalidOperationException("Você já realizou o check-in neste espaço.");
        }

        currentReservation.CheckInAt = DateTime.UtcNow;
        currentReservation.Status = ReservationStatus.CheckedIn;

        await _reservationRepository.UpdateAsync(currentReservation);
        await _unitOfWork.CommitAsync();

        return true;
    }

    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371e3; // Raio da Terra em metros
        var phi1 = lat1 * Math.PI / 180;
        var phi2 = lat2 * Math.PI / 180;
        var deltaPhi = (lat2 - lat1) * Math.PI / 180;
        var deltaLambda = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                Math.Cos(phi1) * Math.Cos(phi2) *
                Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }
}