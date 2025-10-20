namespace Sinema.Application.Abstractions.Export;

/// <summary>
/// Interface for exporting data to Excel format
/// </summary>
public interface IExcelExporter
{
    /// <summary>
    /// Exports a report to Excel format and returns the file content
    /// </summary>
    /// <param name="reportData">The report data to export</param>
    /// <param name="sheetName">Name of the Excel sheet</param>
    /// <param name="fileName">Base name for the file (without extension)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the file bytes and suggested filename</returns>
    Task<(byte[] FileBytes, string FileName)> ExportToExcelAsync<T>(
        IEnumerable<T> reportData, 
        string sheetName, 
        string fileName, 
        CancellationToken cancellationToken = default) where T : class;
}
