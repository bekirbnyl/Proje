using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;
using Sinema.Infrastructure.Services.Reservations;
using Sinema.Infrastructure.Persistence;
using Sinema.Application.Abstractions.Settings;

namespace Sinema.UnitTests.Reservations.Holds;

/// <summary>
/// Unit tests for CreateSeatHolds functionality
/// </summary>
public class CreateSeatHolds_Tests
{
    private readonly Mock<ISeatHoldRepository> _mockSeatHoldRepository;
    private readonly Mock<IScreeningRepository> _mockScreeningRepository;
    private readonly Mock<ISeatLayoutRepository> _mockSeatLayoutRepository;
    private readonly Mock<IReservationRepository> _mockReservationRepository;
    private readonly Mock<ITicketRepository> _mockTicketRepository;
    private readonly Mock<IAppSettingsReader> _mockSettingsReader;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<SeatHoldService>> _mockLogger;
    private readonly Mock<SinemaDbContext> _mockContext;
    private readonly SeatHoldService _service;

    public CreateSeatHolds_Tests()
    {
        _mockSeatHoldRepository = new Mock<ISeatHoldRepository>();
        _mockScreeningRepository = new Mock<IScreeningRepository>();
        _mockSeatLayoutRepository = new Mock<ISeatLayoutRepository>();
        _mockReservationRepository = new Mock<IReservationRepository>();
        _mockTicketRepository = new Mock<ITicketRepository>();
        _mockSettingsReader = new Mock<IAppSettingsReader>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<SeatHoldService>>();
        _mockContext = new Mock<SinemaDbContext>();

        // Setup default configuration
        _mockConfiguration.Setup(c => c.GetValue<int>("SeatHold:DefaultTtlSeconds", 120))
            .Returns(120);

        _service = new SeatHoldService(
            _mockSeatHoldRepository.Object,
            _mockScreeningRepository.Object,
            _mockSeatLayoutRepository.Object,
            _mockReservationRepository.Object,
            _mockTicketRepository.Object,
            _mockSettingsReader.Object,
            _mockConfiguration.Object,
            _mockLogger.Object,
            _mockContext.Object);
    }

    [Fact]
    public async Task CreateHoldsAsync_WithValidData_ShouldSucceed()
    {
        // Arrange
        var screeningId = Guid.NewGuid();
        var seatIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var clientToken = "test-client-token";
        var userId = Guid.NewGuid();

        var screening = new Screening
        {
            Id = screeningId,
            StartAt = DateTime.UtcNow.AddHours(2), // 2 hours in future
            SeatLayoutId = Guid.NewGuid()
        };

        var seatLayout = new SeatLayout
        {
            Id = screening.SeatLayoutId,
            HallId = Guid.NewGuid()
        };

        _mockScreeningRepository.Setup(r => r.GetByIdAsync(screeningId, default))
            .ReturnsAsync(screening);

        _mockSeatLayoutRepository.Setup(r => r.GetByIdAsync(screening.SeatLayoutId, default))
            .ReturnsAsync(seatLayout);

        _mockSeatHoldRepository.Setup(r => r.GetActiveHoldsForSeatsAsync(screeningId, seatIds, default))
            .ReturnsAsync(new List<SeatHold>());

        _mockSeatHoldRepository.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SeatHold>>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateHoldsAsync(screeningId, seatIds, clientToken, userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        foreach (var hold in result)
        {
            hold.ScreeningId.Should().Be(screeningId);
            hold.ClientToken.Should().Be(clientToken);
            hold.UserId.Should().Be(userId);
            hold.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        _mockSeatHoldRepository.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<SeatHold>>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateHoldsAsync_WithinT60_ShouldThrowException()
    {
        // Arrange
        var screeningId = Guid.NewGuid();
        var seatIds = new[] { Guid.NewGuid() };
        var clientToken = "test-client-token";

        var screening = new Screening
        {
            Id = screeningId,
            StartAt = DateTime.UtcNow.AddMinutes(30), // Within T-60 window
            SeatLayoutId = Guid.NewGuid()
        };

        _mockScreeningRepository.Setup(r => r.GetByIdAsync(screeningId, default))
            .ReturnsAsync(screening);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateHoldsAsync(screeningId, seatIds, clientToken));

        exception.Message.Should().Contain("within 60 minutes");
    }

    [Fact]
    public async Task CreateHoldsAsync_WithExistingHolds_ShouldThrowException()
    {
        // Arrange
        var screeningId = Guid.NewGuid();
        var seatIds = new[] { Guid.NewGuid() };
        var clientToken = "test-client-token";

        var screening = new Screening
        {
            Id = screeningId,
            StartAt = DateTime.UtcNow.AddHours(2),
            SeatLayoutId = Guid.NewGuid()
        };

        var seatLayout = new SeatLayout
        {
            Id = screening.SeatLayoutId,
            HallId = Guid.NewGuid()
        };

        var existingHold = new SeatHold
        {
            Id = Guid.NewGuid(),
            ScreeningId = screeningId,
            SeatId = seatIds[0],
            ClientToken = "other-client",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        _mockScreeningRepository.Setup(r => r.GetByIdAsync(screeningId, default))
            .ReturnsAsync(screening);

        _mockSeatLayoutRepository.Setup(r => r.GetByIdAsync(screening.SeatLayoutId, default))
            .ReturnsAsync(seatLayout);

        _mockSeatHoldRepository.Setup(r => r.GetActiveHoldsForSeatsAsync(screeningId, seatIds, default))
            .ReturnsAsync(new[] { existingHold });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateHoldsAsync(screeningId, seatIds, clientToken));

        exception.Message.Should().Contain("already held");
    }

    [Fact]
    public async Task CreateHoldsAsync_WithEmptyClientToken_ShouldThrowException()
    {
        // Arrange
        var screeningId = Guid.NewGuid();
        var seatIds = new[] { Guid.NewGuid() };
        var clientToken = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateHoldsAsync(screeningId, seatIds, clientToken));

        exception.Message.Should().Contain("Client token is required");
    }

    [Fact]
    public async Task CreateHoldsAsync_WithEmptySeatIds_ShouldThrowException()
    {
        // Arrange
        var screeningId = Guid.NewGuid();
        var seatIds = Array.Empty<Guid>();
        var clientToken = "test-client-token";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateHoldsAsync(screeningId, seatIds, clientToken));

        exception.Message.Should().Contain("At least one seat ID is required");
    }
}
