using MediatR;
using Sinema.Application.DTOs.Tickets;

namespace Sinema.Application.Features.Tickets.Queries.GetTicketById;

/// <summary>
/// Query to get ticket details by ID
/// </summary>
public record GetTicketByIdQuery(Guid TicketId) : IRequest<GetTicketResponse?>;
