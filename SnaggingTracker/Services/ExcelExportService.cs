using ClosedXML.Excel;
using SnaggingTracker.Helpers;
using SnaggingTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnaggingTracker.Services
{
    /// <summary>
    /// Concrete implementation of IExcelExportService using ClosedXML.
    /// Produces professionally formatted .xlsx workbooks with up to 3 sheets.
    /// </summary>
    public class ExcelExportService : IExcelExportService
    {
        // Brand colors (hex without #)
        private static readonly XLColor HeaderBg    = XLColor.FromHtml("#0D1B2A");
        private static readonly XLColor HeaderFg    = XLColor.FromHtml("#FFFFFF");
        private static readonly XLColor AccentAmber = XLColor.FromHtml("#E67E22");
        private static readonly XLColor AccentTeal  = XLColor.FromHtml("#0E6655");
        private static readonly XLColor RowAlt      = XLColor.FromHtml("#F4F6F7");
        private static readonly XLColor RedHigh     = XLColor.FromHtml("#FADBD8");
        private static readonly XLColor AmberMed    = XLColor.FromHtml("#FDEBD0");
        private static readonly XLColor GreenLow    = XLColor.FromHtml("#D5F5E3");
        private static readonly XLColor GreenClosed = XLColor.FromHtml("#D5F5E3");
        private static readonly XLColor BlueInProg  = XLColor.FromHtml("#D6EAF8");

        public Task<bool> ExportFullReportAsync(List<SnagIssue> issues, string filePath)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var wb = new XLWorkbook();
                    AddSnagListSheet(wb, issues);
                    AddClosureRateSheet(wb, issues);
                    AddContractorSheet(wb, issues);
                    wb.SaveAs(filePath);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Excel] Full report error: {ex.Message}");
                    return false;
                }
            });
        }

        public Task<bool> ExportSnagListAsync(List<SnagIssue> issues, string filePath)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var wb = new XLWorkbook();
                    AddSnagListSheet(wb, issues);
                    wb.SaveAs(filePath);
                    return true;
                }
                catch { return false; }
            });
        }

        public Task<bool> ExportContractorReportAsync(List<SnagIssue> issues, string filePath)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var wb = new XLWorkbook();
                    AddContractorSheet(wb, issues);
                    wb.SaveAs(filePath);
                    return true;
                }
                catch { return false; }
            });
        }

        // ── Sheet 1: Raw Snag List ────────────────────────────────────────
        private void AddSnagListSheet(XLWorkbook wb, List<SnagIssue> issues)
        {
            var ws = wb.Worksheets.Add("Snag List");

            // Title row
            ws.Cell(1, 1).Value = "SITE ISSUE & SNAGGING TRACKER — Full Snag List";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Font.FontColor = AccentAmber;
            ws.Range(1, 1, 1, 10).Merge();

            ws.Cell(2, 1).Value = $"Exported: {DateTime.Now:dd MMM yyyy HH:mm}   |   Total Issues: {issues.Count}";
            ws.Cell(2, 1).Style.Font.Italic = true;
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
            ws.Range(2, 1, 2, 10).Merge();

            // Header row
            string[] headers = { "Ref No.", "Title", "Location", "Priority", "Status",
                                  "Responsible Party", "Date Logged", "Due Date", "Date Resolved", "Remarks" };
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = ws.Cell(4, c + 1);
                cell.Value = headers[c];
                cell.Style.Fill.BackgroundColor = HeaderBg;
                cell.Style.Font.FontColor = HeaderFg;
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.White;
            }

            // Data rows
            for (int i = 0; i < issues.Count; i++)
            {
                var issue = issues[i];
                int row = i + 5;
                bool isAlt = i % 2 == 1;

                ws.Cell(row, 1).Value  = issue.ReferenceNumber;
                ws.Cell(row, 2).Value  = issue.Title;
                ws.Cell(row, 3).Value  = issue.Location;
                ws.Cell(row, 4).Value  = issue.Priority.ToString();
                ws.Cell(row, 5).Value  = issue.Status == IssueStatus.InProgress ? "In Progress" : issue.Status.ToString();
                ws.Cell(row, 6).Value  = issue.ResponsibleParty;
                ws.Cell(row, 7).Value  = issue.DateLogged.ToString("dd/MM/yyyy");
                ws.Cell(row, 8).Value  = issue.DueDate.HasValue  ? issue.DueDate.Value.ToString("dd/MM/yyyy")      : "—";
                ws.Cell(row, 9).Value  = issue.DateResolved.HasValue ? issue.DateResolved.Value.ToString("dd/MM/yyyy") : "—";
                ws.Cell(row, 10).Value = issue.Remarks;

                // Priority color on col 4
                var priorityCell = ws.Cell(row, 4);
                priorityCell.Style.Fill.BackgroundColor = issue.Priority switch
                {
                    IssuePriority.High   => RedHigh,
                    IssuePriority.Medium => AmberMed,
                    IssuePriority.Low    => GreenLow,
                    _ => XLColor.White
                };
                priorityCell.Style.Font.Bold = issue.Priority == IssuePriority.High;

                // Status color on col 5
                var statusCell = ws.Cell(row, 5);
                statusCell.Style.Fill.BackgroundColor = issue.Status switch
                {
                    IssueStatus.Open       => RedHigh,
                    IssueStatus.InProgress => BlueInProg,
                    IssueStatus.Closed     => GreenClosed,
                    _ => XLColor.White
                };

                // Alternating row background for non-colored cells
                if (isAlt)
                {
                    foreach (int col in new[] { 1, 2, 3, 6, 7, 8, 9, 10 })
                        ws.Cell(row, col).Style.Fill.BackgroundColor = RowAlt;
                }

                // Thin border on all data cells
                ws.Range(row, 1, row, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Hair;
                ws.Range(row, 1, row, 10).Style.Border.OutsideBorderColor = XLColor.LightGray;
            }

            // Column widths
            ws.Column(1).Width = 12;
            ws.Column(2).Width = 30;
            ws.Column(3).Width = 22;
            ws.Column(4).Width = 12;
            ws.Column(5).Width = 14;
            ws.Column(6).Width = 24;
            ws.Column(7).Width = 14;
            ws.Column(8).Width = 14;
            ws.Column(9).Width = 16;
            ws.Column(10).Width = 40;

            ws.Column(2).Style.Alignment.WrapText = true;
            ws.Column(10).Style.Alignment.WrapText = true;
            ws.SheetView.FreezeRows(4);
        }

        // ── Sheet 2: Closure Rate ─────────────────────────────────────────
        private void AddClosureRateSheet(XLWorkbook wb, List<SnagIssue> issues)
        {
            var ws = wb.Worksheets.Add("Closure Rate");

            ws.Cell(1, 1).Value = "CLOSURE RATE REPORT";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Font.FontColor = AccentAmber;
            ws.Range(1, 1, 1, 4).Merge();

            ws.Cell(2, 1).Value = $"Generated: {DateTime.Now:dd MMM yyyy HH:mm}";
            ws.Cell(2, 1).Style.Font.Italic = true;
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
            ws.Range(2, 1, 2, 4).Merge();

            // Headers
            string[] headers = { "Status", "Count", "Percentage (%)", "Visual" };
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = ws.Cell(4, c + 1);
                cell.Value = headers[c];
                cell.Style.Fill.BackgroundColor = HeaderBg;
                cell.Style.Font.FontColor = HeaderFg;
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int total      = issues.Count;
            int openCount  = issues.Count(i => i.Status == IssueStatus.Open);
            int inProgCount= issues.Count(i => i.Status == IssueStatus.InProgress);
            int closedCount= issues.Count(i => i.Status == IssueStatus.Closed);

            var rows = new[]
            {
                ("Open",        openCount,   RedHigh),
                ("In Progress", inProgCount, BlueInProg),
                ("Closed",      closedCount, GreenClosed),
                ("TOTAL",       total,       XLColor.LightGray),
            };

            for (int i = 0; i < rows.Length; i++)
            {
                var (label, count, color) = rows[i];
                int row = i + 5;
                double pct = total > 0 ? Math.Round((double)count / total * 100, 1) : 0;
                int barLen = (int)(pct / 5); // max 20 chars for 100%

                ws.Cell(row, 1).Value = label;
                ws.Cell(row, 2).Value = count;
                ws.Cell(row, 3).Value = total > 0 ? pct : 0;
                ws.Cell(row, 4).Value = new string('█', barLen);

                ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = color;
                ws.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(row, 1).Style.Font.Bold = label == "TOTAL";
                ws.Cell(row, 4).Style.Font.FontColor = label == "Closed" ? AccentTeal :
                                                        label == "Open"   ? XLColor.Red :
                                                        XLColor.FromHtml("#1A5276");
            }

            // KPI summary
            double closureRate = total > 0 ? Math.Round((double)closedCount / total * 100, 1) : 0;
            ws.Cell(10, 1).Value = "Overall Closure Rate:";
            ws.Cell(10, 1).Style.Font.Bold = true;
            ws.Cell(10, 2).Value = $"{closureRate}%";
            ws.Cell(10, 2).Style.Font.Bold = true;
            ws.Cell(10, 2).Style.Font.FontColor = closureRate >= 80 ? AccentTeal : AccentAmber;
            ws.Cell(10, 2).Style.Font.FontSize = 14;

            ws.Column(1).Width = 18;
            ws.Column(2).Width = 10;
            ws.Column(3).Width = 18;
            ws.Column(4).Width = 28;
        }

        // ── Sheet 3: Contractor Performance ──────────────────────────────
        private void AddContractorSheet(XLWorkbook wb, List<SnagIssue> issues)
        {
            var ws = wb.Worksheets.Add("Contractor Performance");

            ws.Cell(1, 1).Value = "CONTRACTOR PERFORMANCE REPORT";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Font.FontColor = AccentAmber;
            ws.Range(1, 1, 1, 7).Merge();

            ws.Cell(2, 1).Value = $"Generated: {DateTime.Now:dd MMM yyyy HH:mm}";
            ws.Cell(2, 1).Style.Font.Italic = true;
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
            ws.Range(2, 1, 2, 7).Merge();

            string[] headers = { "Responsible Party", "Open", "In Progress", "Closed", "Total Issues", "Closure Rate (%)", "Performance" };
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = ws.Cell(4, c + 1);
                cell.Value = headers[c];
                cell.Style.Fill.BackgroundColor = HeaderBg;
                cell.Style.Font.FontColor = HeaderFg;
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            var grouped = issues
                .GroupBy(i => string.IsNullOrWhiteSpace(i.ResponsibleParty) ? "Unassigned" : i.ResponsibleParty)
                .OrderBy(g => g.Key)
                .ToList();

            for (int i = 0; i < grouped.Count; i++)
            {
                var grp = grouped[i];
                int row = i + 5;
                bool isAlt = i % 2 == 1;

                int open   = grp.Count(x => x.Status == IssueStatus.Open);
                int inProg = grp.Count(x => x.Status == IssueStatus.InProgress);
                int closed = grp.Count(x => x.Status == IssueStatus.Closed);
                int total  = grp.Count();
                double cr  = total > 0 ? Math.Round((double)closed / total * 100, 1) : 0;

                ws.Cell(row, 1).Value = grp.Key;
                ws.Cell(row, 2).Value = open;
                ws.Cell(row, 3).Value = inProg;
                ws.Cell(row, 4).Value = closed;
                ws.Cell(row, 5).Value = total;
                ws.Cell(row, 6).Value = cr;
                ws.Cell(row, 7).Value = cr >= 80 ? "✓ On Track" : cr >= 50 ? "⚠ At Risk" : "✗ Behind";

                var perfColor = cr >= 80 ? GreenClosed : cr >= 50 ? AmberMed : RedHigh;
                ws.Cell(row, 7).Style.Fill.BackgroundColor = perfColor;
                ws.Cell(row, 7).Style.Font.Bold = true;
                ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                if (isAlt)
                {
                    foreach (int col in new[] { 1, 2, 3, 4, 5, 6 })
                        ws.Cell(row, col).Style.Fill.BackgroundColor = RowAlt;
                }

                ws.Range(row, 1, row, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Hair;
            }

            ws.Column(1).Width = 28;
            ws.Column(2).Width = 10;
            ws.Column(3).Width = 14;
            ws.Column(4).Width = 10;
            ws.Column(5).Width = 14;
            ws.Column(6).Width = 18;
            ws.Column(7).Width = 16;
            ws.SheetView.FreezeRows(4);
        }
    }
}
