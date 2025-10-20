using FluentValidation;
using Sinema.Domain.Enums;

namespace Sinema.Application.Features.Tickets.Commands.SellTicket;

/// <summary>
/// Validator for SellTicketCommand
/// </summary>
public class SellTicketValidator : AbstractValidator<SellTicketCommand>
{
    public SellTicketValidator()
    {
        RuleFor(x => x.Request.ScreeningId)
            .NotEmpty()
            .WithMessage("Screening ID is required");

        RuleFor(x => x.Request.Channel)
            .IsInEnum()
            .WithMessage("Valid channel is required");

        RuleFor(x => x.Request.PaymentMethod)
            .IsInEnum()
            .WithMessage("Valid payment method is required");

        // Either ReservationId or Items must be provided, but not both
        RuleFor(x => x.Request)
            .Must(request => (request.ReservationId.HasValue && (request.Items == null || !request.Items.Any())) ||
                           (!request.ReservationId.HasValue && request.Items != null && request.Items.Any()))
            .WithMessage("Either ReservationId or Items must be provided, but not both");

        // When Items are provided, validate them
        RuleFor(x => x.Request.Items)
            .Must(items => items == null || items.Count <= 10)
            .WithMessage("Maximum 10 items allowed per transaction")
            .When(x => x.Request.Items != null);

        RuleForEach(x => x.Request.Items)
            .SetValidator(new SellTicketItemValidator())
            .When(x => x.Request.Items != null);

        // For reservation-based sales, client token is recommended
        RuleFor(x => x.Request.ClientToken)
            .NotEmpty()
            .WithMessage("Client token is required for reservation-based sales")
            .When(x => x.Request.ReservationId.HasValue);

        // Member credit payments require member ID
        RuleFor(x => x.Request.MemberId)
            .NotEmpty()
            .WithMessage("Member ID is required for member credit payments")
            .When(x => x.Request.PaymentMethod == PaymentMethod.MemberCredit);
    }
}

/// <summary>
/// Validator for individual ticket items
/// </summary>
public class SellTicketItemValidator : AbstractValidator<DTOs.Tickets.SellTicketItemRequest>
{
    public SellTicketItemValidator()
    {
        RuleFor(x => x.SeatId)
            .NotEmpty()
            .WithMessage("Seat ID is required");

        RuleFor(x => x.TicketType)
            .IsInEnum()
            .WithMessage("Valid ticket type is required");

        // VIP guest flag should only be true for VIPGuest ticket type
        RuleFor(x => x.IsVipGuest)
            .Equal(false)
            .WithMessage("IsVipGuest should only be true for VIPGuest ticket type")
            .When(x => x.TicketType != TicketType.VIPGuest);
    }
}
