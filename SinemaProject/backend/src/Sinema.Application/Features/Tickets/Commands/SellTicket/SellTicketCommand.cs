using MediatR;
using Sinema.Application.DTOs.Tickets;

namespace Sinema.Application.Features.Tickets.Commands.SellTicket;

/// <summary>
/// Command to sell tickets either from an existing reservation or directly
/// </summary>
public record SellTicketCommand(SellTicketRequest Request) : IRequest<SellTicketResponse>;
