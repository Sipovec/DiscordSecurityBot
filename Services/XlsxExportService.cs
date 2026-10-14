using ClosedXML.Excel;
using Discord;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace DiscordSecurityBot.Services;

public interface IXlsxExportService
{
    Task<string> SaveXlsxAsync(
        IGuild guild,
        string prefix,
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<object?>> rows);
}

public sealed class XlsxExportService(
    ILogger<XlsxExportService> logger) : IXlsxExportService
{
    private readonly ILogger<XlsxExportService> _logger = logger;

    public async Task<string> SaveXlsxAsync(
        IGuild guild,
        string prefix,
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<object?>> rows)
    {
        if (headers.Count == 0)
        {
            throw new ArgumentException(
                "At least one column is required.", nameof(headers));
        }

        string exportDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Exports");

        Directory.CreateDirectory(exportDir);

        string safeGuildName = string.Concat(
            guild.Name.Split(Path.GetInvalidFileNameChars()));

        if (string.IsNullOrWhiteSpace(safeGuildName))
        {
            safeGuildName = guild.Id.ToString(
                CultureInfo.InvariantCulture);
        }

        string safePrefix = string.Concat(
            prefix.Split(Path.GetInvalidFileNameChars()));

        if (string.IsNullOrWhiteSpace(safePrefix))
        {
            safePrefix = "export";
        }

        string suffix = Guid.NewGuid().ToString("N")[..8];
        string fileName =
            $"{safePrefix}_{safeGuildName}_" +
            $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}_{suffix}.xlsx";

        string filePath = Path.Combine(exportDir, fileName);

        using XLWorkbook workbook = new ();
        IXLWorksheet worksheet = workbook.Worksheets.Add("Export");

        // Заголовки
        for (int col = 0; col < headers.Count; col++)
        {
            worksheet.Cell(1, col + 1).Value = headers[col];
        }

        IXLRange headerRange = worksheet.Range(
            1, 1, 1, headers.Count);

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // Данные
        int rowNumber = 2;

        foreach (IReadOnlyList<object?> row in rows)
        {
            if (row.Count != headers.Count)
            {
                throw new ArgumentException(
                    $"Row {rowNumber} has {row.Count} values, " +
                    $"but {headers.Count} columns are expected.");
            }

            for (int col = 0; col < row.Count; col++)
            {
                IXLCell cell = worksheet.Cell(rowNumber, col + 1);

                switch (row[col])
                {
                    case null:
                        cell.Value = string.Empty;
                        break;

                    case string text:
                        // Строка хранится как текст, а не формула.
                        cell.Value = text;
                        cell.Style.NumberFormat.Format = "@";
                        break;

                    case DateTimeOffset date:
                        cell.Value = date.UtcDateTime;
                        cell.Style.NumberFormat.Format =
                            "yyyy-mm-dd hh:mm:ss";
                        break;

                    case DateTime date:
                        cell.Value = date;
                        cell.Style.NumberFormat.Format =
                            "yyyy-mm-dd hh:mm:ss";
                        break;

                    default:
                        cell.Value = XLCellValue.FromObject(
                            row[col]!,
                            CultureInfo.InvariantCulture);
                        break;
                }
            }

            rowNumber++;
        }

        worksheet.SheetView.FreezeRows(1);

        if (rowNumber > 2)
        {
            worksheet.Range(
                1, 1, rowNumber - 1, headers.Count
            ).SetAutoFilter();
        }

        // Подбор ширины без чрезмерно широких столбцов.
        for (int col = 1; col <= headers.Count; col++)
        {
            var column = worksheet.Column(col);
            column.AdjustToContents();

            if (column.Width > 60)
                column.Width = 60;
        }

        // Сериализация книги и асинхронное сохранение файла.
        using MemoryStream stream = new ();
        workbook.SaveAs(stream);

        await File.WriteAllBytesAsync(
            filePath, stream.ToArray());

        _logger.LogInformation(
            "Saved XLSX export: {FilePath}", filePath);

        return filePath;
    }
}
