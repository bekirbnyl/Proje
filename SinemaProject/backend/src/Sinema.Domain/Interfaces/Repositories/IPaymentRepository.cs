using Sinema.Domain.Entities;
using Sinema.Domain.Enums;

namespace Sinema.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Payment entities
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Gets a payment by its ID
    /// </summary>
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments by date range
    /// </summary>
    Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments by payment method
    /// </summary>
    Task<IEnumerable<Payment>> GetByMethodAsync(PaymentMethod method, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments by status
    /// </summary>
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payment by external reference
    /// </summary>
    Task<Payment?> GetByExternalReferenceAsync(string externalReference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new payment
    /// </summary>
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing payment
    /// </summary>
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a payment
    /// </summary>
    Task DeleteAsync(Payment payment, CancellationToken cancellationToken = default);
}
