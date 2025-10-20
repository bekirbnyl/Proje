using Microsoft.Extensions.Logging;
using Sinema.Application.Abstractions.Payments;
using Sinema.Domain.Enums;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Services.Payments;

/// <summary>
/// Stub implementation of payment gateway for testing and development
/// </summary>
public class PaymentGatewayStub : IPaymentGateway
{
    private readonly ILogger<PaymentGatewayStub> _logger;
    private readonly IMemberRepository _memberRepository;

    public PaymentGatewayStub(ILogger<PaymentGatewayStub> logger, IMemberRepository memberRepository)
    {
        _logger = logger;
        _memberRepository = memberRepository;
    }

    public async Task<PaymentResult> AuthorizeAndCaptureAsync(
        decimal amount, 
        PaymentMethod method, 
        Guid? memberId = null, 
        Dictionary<string, string>? metadata = null)
    {
        _logger.LogInformation("Processing payment: Amount={Amount}, Method={Method}, MemberId={MemberId}", 
            amount, method, memberId);

        // Simulate network delay
        await Task.Delay(Random.Shared.Next(100, 500));

        // Special test cases for failure simulation
        if (metadata?.ContainsKey("simulate_failure") == true)
        {
            _logger.LogWarning("Simulating payment failure");
            return PaymentResult.Failure("Simulated payment failure for testing");
        }

        // Validate member credit if using MemberCredit payment method
        if (method == PaymentMethod.MemberCredit)
        {
            if (!memberId.HasValue)
            {
                return PaymentResult.Failure("Member ID is required for member credit payments");
            }

            var member = await _memberRepository.GetByIdAsync(memberId.Value);
            if (member == null)
            {
                return PaymentResult.Failure("Member not found");
            }

            var availableCredit = await _memberRepository.GetAvailableCreditAsync(memberId.Value);
            if (availableCredit < amount)
            {
                return PaymentResult.Failure($"Insufficient credit balance. Available: {availableCredit:C}, Required: {amount:C}");
            }
        }

        // Generate a fake transaction ID
        var transactionId = method switch
        {
            PaymentMethod.Cash => $"CASH_{DateTime.UtcNow:yyyyMMddHHmmss}_{Random.Shared.Next(1000, 9999)}",
            PaymentMethod.CreditCard => $"CC_{DateTime.UtcNow:yyyyMMddHHmmss}_{Random.Shared.Next(1000, 9999)}",
            PaymentMethod.BankTransfer => $"BT_{DateTime.UtcNow:yyyyMMddHHmmss}_{Random.Shared.Next(1000, 9999)}",
            PaymentMethod.MemberCredit => $"MC_{DateTime.UtcNow:yyyyMMddHHmmss}_{Random.Shared.Next(1000, 9999)}",
            _ => $"UNK_{DateTime.UtcNow:yyyyMMddHHmmss}_{Random.Shared.Next(1000, 9999)}"
        };

        var resultMetadata = new Dictionary<string, string>
        {
            ["payment_method"] = method.ToString(),
            ["amount"] = amount.ToString("F2"),
            ["timestamp"] = DateTime.UtcNow.ToString("O"),
            ["gateway"] = "stub"
        };

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                resultMetadata.TryAdd($"request_{kvp.Key}", kvp.Value);
            }
        }

        _logger.LogInformation("Payment successful: TransactionId={TransactionId}", transactionId);
        return PaymentResult.Success(transactionId, resultMetadata);
    }
}
