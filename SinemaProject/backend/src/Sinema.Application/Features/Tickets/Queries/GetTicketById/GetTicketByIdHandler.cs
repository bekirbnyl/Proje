using MediatR;
using Microsoft.Extensions.Logging;
using Sinema.Application.DTOs.Tickets;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Tickets.Queries.GetTicketById;

/// <summary>
/// Handler for GetTicketByIdQuery
/// </summary>
public class GetTicketByIdHandler : IRequestHandler<GetTicketByIdQuery, GetTicketResponse?>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IHallRepository _hallRepository;
    private readonly ILogger<GetTicketByIdHandler> _logger;

    public GetTicketByIdHandler(
        ITicketRepository ticketRepository,
        ISeatRepository seatRepository,
        IScreeningRepository screeningRepository,
        IMovieRepository movieRepository,
        IHallRepository hallRepository,
        ILogger<GetTicketByIdHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _seatRepository = seatRepository;
        _screeningRepository = screeningRepository;
        _movieRepository = movieRepository;
        _hallRepository = hallRepository;
        _logger = logger;
    }

    public async Task<GetTicketResponse?> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
        {
            _logger.LogWarning("Ticket with ID {TicketId} not found", request.TicketId);
            return null;
        }

        // Get seat information
        var seat = await _seatRepository.GetByIdAsync(ticket.SeatId, cancellationToken);
        if (seat == null)
        {
            _logger.LogWarning("Seat with ID {SeatId} not found for ticket {TicketId}", 
                ticket.SeatId, ticket.Id);
            throw new InvalidOperationException($"Seat with ID '{ticket.SeatId}' not found");
        }

        // Get screening information
        var screening = await _screeningRepository.GetByIdAsync(ticket.ScreeningId, cancellationToken);
        if (screening == null)
        {
            _logger.LogWarning("Screening with ID {ScreeningId} not found for ticket {TicketId}", 
                ticket.ScreeningId, ticket.Id);
            throw new InvalidOperationException($"Screening with ID '{ticket.ScreeningId}' not found");
        }

        // Get movie information
        var movie = await _movieRepository.GetByIdAsync(screening.MovieId, cancellationToken);
        var movieTitle = movie?.Title ?? "Unknown Movie";

        // Get hall information
        var hall = await _hallRepository.GetByIdAsync(screening.HallId, cancellationToken);
        var hallName = hall?.Name ?? "Unknown Hall";

        return new GetTicketResponse
        {
            TicketId = ticket.Id,
            TicketCode = ticket.TicketCode ?? string.Empty,
            ScreeningId = ticket.ScreeningId,
            SeatId = ticket.SeatId,
            TicketType = ticket.Type,
            Channel = ticket.Channel,
            Price = ticket.Price,
            SoldAt = ticket.SoldAt,
            AppliedPricingJson = ticket.AppliedPricingJson,
            Seat = new SeatInfo
            {
                Row = seat.Row,
                Col = seat.Col,
                Label = seat.Label
            },
            Screening = new ScreeningInfo
            {
                StartAt = screening.StartAt,
                MovieTitle = movieTitle,
                HallName = hallName
            }
        };
    }
}
