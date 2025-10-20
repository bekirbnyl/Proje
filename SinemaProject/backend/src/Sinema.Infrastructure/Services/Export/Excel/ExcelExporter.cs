using OfficeOpenXml;
using OfficeOpenXml.Style;
using Sinema.Application.Abstractions.Export;
using System.Drawing;
using System.Globalization;
using System.Reflection;

namespace Sinema.Infrastructure.Services.Export.Excel;

/// <summary>
/// Excel export service using EPPlus
/// </summary>
public class ExcelExporter : IExcelExporter
{
    /// <summary>
    /// Exports a report to Excel format and returns the file content
    /// </summary>
    /// <param name="reportData">The report data to export</param>
    /// <param name="sheetName">Name of the Excel sheet</param>
    /// <param name="fileName">Base name for the file (without extension)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the file bytes and suggested filename</returns>
    public async Task<(byte[] FileBytes, string FileName)> ExportToExcelAsync<T>(
        IEnumerable<T> reportData, 
        string sheetName, 
        string fileName, 
        CancellationToken cancellationToken = default) where T : class
    {
        // Set EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(sheetName);

        var dataList = reportData.ToList();
        if (dataList.Count == 0)
        {
            // Create empty sheet with headers only
            worksheet.Cells[1, 1].Value = "No data available for the selected criteria";
            worksheet.Cells[1, 1].Style.Font.Bold = true;
        }
        else
        {
            // Get properties for columns
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                .ToArray();

            // Add headers
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var headerName = GetDisplayName(property);
                worksheet.Cells[1, i + 1].Value = headerName;
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            // Add data rows
            for (int row = 0; row < dataList.Count; row++)
            {
                var item = dataList[row];
                for (int col = 0; col < properties.Length; col++)
                {
                    var property = properties[col];
                    var value = property.GetValue(item);
                    var cell = worksheet.Cells[row + 2, col + 1];

                    if (value != null)
                    {
                        // Format based on data type
                        if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                        {
                            cell.Value = value;
                            cell.Style.Numberformat.Format = "yyyy-mm-dd";
                        }
                        else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?) ||
                                property.PropertyType == typeof(double) || property.PropertyType == typeof(double?))
                        {
                            cell.Value = value;
                            if (property.Name.Contains("Price", StringComparison.OrdinalIgnoreCase) ||
                                property.Name.Contains("Gross", StringComparison.OrdinalIgnoreCase) ||
                                property.Name.Contains("Net", StringComparison.OrdinalIgnoreCase) ||
                                property.Name.Contains("Discount", StringComparison.OrdinalIgnoreCase) ||
                                property.Name.Contains("Total", StringComparison.OrdinalIgnoreCase))
                            {
                                cell.Style.Numberformat.Format = "₺#,##0.00";
                            }
                            else if (property.Name.Contains("Rate", StringComparison.OrdinalIgnoreCase) ||
                                    property.Name.Contains("Ratio", StringComparison.OrdinalIgnoreCase) ||
                                    property.Name.Contains("Occupancy", StringComparison.OrdinalIgnoreCase))
                            {
                                cell.Style.Numberformat.Format = "0.00%";
                            }
                            else
                            {
                                cell.Style.Numberformat.Format = "#,##0.00";
                            }
                        }
                        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?) ||
                                property.PropertyType == typeof(long) || property.PropertyType == typeof(long?))
                        {
                            cell.Value = value;
                            cell.Style.Numberformat.Format = "#,##0";
                        }
                        else
                        {
                            cell.Value = value.ToString();
                        }
                    }
                }
            }

            // Add totals row for numeric columns if appropriate
            AddTotalsRow(worksheet, dataList, properties);
        }

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        // Freeze the header row
        worksheet.View.FreezePanes(2, 1);

        // Add borders to all data
        if (dataList.Count > 0)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                .ToArray();

            var dataRange = worksheet.Cells[1, 1, dataList.Count + 1, properties.Length];
            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        var fileBytes = await package.GetAsByteArrayAsync(cancellationToken);
        var fullFileName = $"{fileName}.xlsx";

        return (fileBytes, fullFileName);
    }

    /// <summary>
    /// Checks if a type is a simple type that can be exported to Excel
    /// </summary>
    private static bool IsSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        
        return underlyingType.IsPrimitive ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(Guid) ||
               underlyingType.IsEnum;
    }

    /// <summary>
    /// Gets the display name for a property
    /// </summary>
    private static string GetDisplayName(PropertyInfo property)
    {
        // Convert PascalCase to Title Case with spaces
        var name = property.Name;
        var result = string.Empty;
        
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
            {
                result += " ";
            }
            result += name[i];
        }

        return result;
    }

    /// <summary>
    /// Adds a totals row for numeric columns
    /// </summary>
    private static void AddTotalsRow<T>(ExcelWorksheet worksheet, List<T> dataList, PropertyInfo[] properties) where T : class
    {
        if (dataList.Count == 0) return;

        var numericProperties = properties
            .Where(p => IsNumericType(p.PropertyType))
            .ToArray();

        if (numericProperties.Length == 0) return;

        var totalRow = dataList.Count + 2;
        
        // Add "Total" label in the first column
        worksheet.Cells[totalRow, 1].Value = "TOTAL";
        worksheet.Cells[totalRow, 1].Style.Font.Bold = true;

        // Calculate totals for numeric columns
        for (int i = 0; i < properties.Length; i++)
        {
            var property = properties[i];
            if (IsNumericType(property.PropertyType) && ShouldSum(property))
            {
                var sum = dataList.Sum(item =>
                {
                    var value = property.GetValue(item);
                    return value switch
                    {
                        decimal d => d,
                        double d => (decimal)d,
                        float f => (decimal)f,
                        int i => i,
                        long l => l,
                        _ => 0m
                    };
                });

                var cell = worksheet.Cells[totalRow, i + 1];
                cell.Value = sum;
                cell.Style.Font.Bold = true;
                
                // Apply same formatting as data cells
                if (property.Name.Contains("Price", StringComparison.OrdinalIgnoreCase) ||
                    property.Name.Contains("Gross", StringComparison.OrdinalIgnoreCase) ||
                    property.Name.Contains("Net", StringComparison.OrdinalIgnoreCase) ||
                    property.Name.Contains("Discount", StringComparison.OrdinalIgnoreCase) ||
                    property.Name.Contains("Total", StringComparison.OrdinalIgnoreCase))
                {
                    cell.Style.Numberformat.Format = "₺#,##0.00";
                }
                else
                {
                    cell.Style.Numberformat.Format = "#,##0";
                }
            }
        }

        // Add border to total row
        var totalRange = worksheet.Cells[totalRow, 1, totalRow, properties.Length];
        totalRange.Style.Border.Top.Style = ExcelBorderStyle.Double;
        totalRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    }

    /// <summary>
    /// Checks if a type is numeric
    /// </summary>
    private static bool IsNumericType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        
        return underlyingType == typeof(decimal) ||
               underlyingType == typeof(double) ||
               underlyingType == typeof(float) ||
               underlyingType == typeof(int) ||
               underlyingType == typeof(long) ||
               underlyingType == typeof(short) ||
               underlyingType == typeof(byte);
    }

    /// <summary>
    /// Determines if a numeric property should be summed in totals
    /// </summary>
    private static bool ShouldSum(PropertyInfo property)
    {
        var name = property.Name.ToLowerInvariant();
        
        // Don't sum rates, ratios, averages, or percentages
        if (name.Contains("rate") || name.Contains("ratio") || 
            name.Contains("avg") || name.Contains("average") ||
            name.Contains("occupancy") || name.Contains("percent"))
        {
            return false;
        }

        // Sum counts, totals, amounts, prices
        return name.Contains("count") || name.Contains("total") || 
               name.Contains("gross") || name.Contains("net") ||
               name.Contains("discount") || name.Contains("amount") ||
               name.Contains("price") || name.Contains("sold") ||
               name.Contains("seats") || name.Contains("ticket");
    }
}
