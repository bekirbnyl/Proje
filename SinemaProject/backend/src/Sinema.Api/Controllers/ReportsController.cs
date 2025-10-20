using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinema.Application.Abstractions.Export;
using Sinema.Application.DTOs.Reports.Deletions;
using Sinema.Application.DTOs.Reports.Memberships;
using Sinema.Application.DTOs.Reports.Sales;
using Sinema.Application.Features.Reports.Deletions;
using Sinema.Application.Features.Reports.Memberships;
using Sinema.Application.Features.Reports.Sales;

namespace Sinema.Api.Controllers;

/// <summary>
/// Controller for generating and downloading reports
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "CanViewReports")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IExcelExporter _excelExporter;
    private readonly IValidator<SalesReportRequest> _salesValidator;
    private readonly IValidator<DeletionsReportRequest> _deletionsValidator;
    private readonly IValidator<MembershipsReportRequest> _membershipsValidator;
    private readonly ILogger<ReportsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ReportsController
    /// </summary>
    public ReportsController(
        IMediator mediator,
        IExcelExporter excelExporter,
        IValidator<SalesReportRequest> salesValidator,
        IValidator<DeletionsReportRequest> deletionsValidator,
        IValidator<MembershipsReportRequest> membershipsValidator,
        ILogger<ReportsController> logger)
    {
        _mediator = mediator;
        _excelExporter = excelExporter;
        _salesValidator = salesValidator;
        _deletionsValidator = deletionsValidator;
        _membershipsValidator = membershipsValidator;
        _logger = logger;
    }

    /// <summary>
    /// Generates a sales report
    /// </summary>
    /// <param name="request">Sales report parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sales report data or Excel file</returns>
    /// <response code="200">Report generated successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("sales")]
    [ProducesResponseType(typeof(SalesReportResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetSalesReport([FromQuery] SalesReportRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sales report requested: {From} to {To}, grain: {Grain}, by: {By}, channel: {Channel}, format: {Format}",
            request.From, request.To, request.Grain, request.By, request.Channel, request.Format);

        // Validate request
        var validationResult = await _salesValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        try
        {
            // Execute query
            var query = GetSalesReportQuery.FromRequest(request);
            var response = await _mediator.Send(query, cancellationToken);

            // Return appropriate format
            if (request.IsExcelFormat)
            {
                return await ExportSalesReportToExcel(response, request, cancellationToken);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales report");
            return StatusCode(500, "An error occurred while generating the report");
        }
    }

    /// <summary>
    /// Generates a deletions report
    /// </summary>
    /// <param name="request">Deletions report parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletions report data or Excel file</returns>
    /// <response code="200">Report generated successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("deletions")]
    [ProducesResponseType(typeof(DeletionsReportResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetDeletionsReport([FromQuery] DeletionsReportRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deletions report requested: {From} to {To}, by: {By}, format: {Format}",
            request.From, request.To, request.By, request.Format);

        // Validate request
        var validationResult = await _deletionsValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        try
        {
            // Execute query
            var query = GetDeletionsReportQuery.FromRequest(request);
            var response = await _mediator.Send(query, cancellationToken);

            // Return appropriate format
            if (request.IsExcelFormat)
            {
                return await ExportDeletionsReportToExcel(response, request, cancellationToken);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating deletions report");
            return StatusCode(500, "An error occurred while generating the report");
        }
    }

    /// <summary>
    /// Generates a memberships report
    /// </summary>
    /// <param name="request">Memberships report parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Memberships report data or Excel file</returns>
    /// <response code="200">Report generated successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("memberships")]
    [ProducesResponseType(typeof(MembershipsReportResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetMembershipsReport([FromQuery] MembershipsReportRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Memberships report requested: {From} to {To}, grain: {Grain}, format: {Format}",
            request.From, request.To, request.Grain, request.Format);

        // Validate request
        var validationResult = await _membershipsValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        try
        {
            // Execute query
            var query = GetMembershipsReportQuery.FromRequest(request);
            var response = await _mediator.Send(query, cancellationToken);

            // Return appropriate format
            if (request.IsExcelFormat)
            {
                return await ExportMembershipsReportToExcel(response, request, cancellationToken);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating memberships report");
            return StatusCode(500, "An error occurred while generating the report");
        }
    }

    /// <summary>
    /// Exports sales report to Excel format
    /// </summary>
    private async Task<IActionResult> ExportSalesReportToExcel(SalesReportResponse response, SalesReportRequest request, CancellationToken cancellationToken)
    {
        var fileName = GenerateSalesFileName(request);
        var (fileBytes, fullFileName) = await _excelExporter.ExportToExcelAsync(response.Data, "Sales", fileName, cancellationToken);

        _logger.LogInformation("Sales report exported to Excel: {FileName}, {RowCount} rows, {FileSize} bytes",
            fullFileName, response.Data.Count, fileBytes.Length);

        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fullFileName);
    }

    /// <summary>
    /// Exports deletions report to Excel format
    /// </summary>
    private async Task<IActionResult> ExportDeletionsReportToExcel(DeletionsReportResponse response, DeletionsReportRequest request, CancellationToken cancellationToken)
    {
        var fileName = GenerateDeletionsFileName(request);
        var (fileBytes, fullFileName) = await _excelExporter.ExportToExcelAsync(response.Data, "Deletions", fileName, cancellationToken);

        _logger.LogInformation("Deletions report exported to Excel: {FileName}, {RowCount} rows, {FileSize} bytes",
            fullFileName, response.Data.Count, fileBytes.Length);

        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fullFileName);
    }

    /// <summary>
    /// Exports memberships report to Excel format
    /// </summary>
    private async Task<IActionResult> ExportMembershipsReportToExcel(MembershipsReportResponse response, MembershipsReportRequest request, CancellationToken cancellationToken)
    {
        var fileName = GenerateMembershipsFileName(request);
        var (fileBytes, fullFileName) = await _excelExporter.ExportToExcelAsync(response.Data, "Memberships", fileName, cancellationToken);

        _logger.LogInformation("Memberships report exported to Excel: {FileName}, {RowCount} rows, {FileSize} bytes",
            fullFileName, response.Data.Count, fileBytes.Length);

        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fullFileName);
    }

    /// <summary>
    /// Generates filename for sales report
    /// </summary>
    private string GenerateSalesFileName(SalesReportRequest request)
    {
        return $"sales_{request.From:yyyy-MM-dd}_{request.To:yyyy-MM-dd}_by-{request.NormalizedBy}_{request.NormalizedGrain}_{request.NormalizedChannel}";
    }

    /// <summary>
    /// Generates filename for deletions report
    /// </summary>
    private string GenerateDeletionsFileName(DeletionsReportRequest request)
    {
        return $"deletions_{request.From:yyyy-MM-dd}_{request.To:yyyy-MM-dd}_by-{request.NormalizedBy}";
    }

    /// <summary>
    /// Generates filename for memberships report
    /// </summary>
    private string GenerateMembershipsFileName(MembershipsReportRequest request)
    {
        return $"memberships_{request.From:yyyy-MM-dd}_{request.To:yyyy-MM-dd}_{request.NormalizedGrain}";
    }
}
