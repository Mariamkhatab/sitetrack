using SnaggingTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnaggingTracker.Services
{
    /// <summary>
    /// Defines the contract for exporting snagging data to Excel workbooks.
    /// Each method maps to a discrete report sheet.
    /// </summary>
    public interface IExcelExportService
    {
        /// <summary>
        /// Exports a full three-sheet workbook to an Excel file:
        ///   Sheet 1 — Full issue log (raw snag list, all fields as columns).
        ///   Sheet 2 — Closure rate summary (Open vs. In Progress vs. Closed counts + percentages).
        ///   Sheet 3 — Contractor performance (issues grouped by Responsible Party).
        /// </summary>
        /// <param name="issues">The full list of issues to export.</param>
        /// <param name="filePath">The full file system path for the output .xlsx file.</param>
        /// <returns>True if export succeeded; false if it failed (e.g. file locked).</returns>
        Task<bool> ExportFullReportAsync(List<SnagIssue> issues, string filePath);

        /// <summary>
        /// Exports only the raw snag list sheet.
        /// Useful for sharing the live issue log without summary analytics.
        /// </summary>
        /// <param name="issues">The full list of issues to export.</param>
        /// <param name="filePath">The full file system path for the output .xlsx file.</param>
        /// <returns>True if export succeeded; false otherwise.</returns>
        Task<bool> ExportSnagListAsync(List<SnagIssue> issues, string filePath);

        /// <summary>
        /// Exports only the contractor performance sheet.
        /// Groups issues by ResponsibleParty and shows status distribution per contractor.
        /// </summary>
        /// <param name="issues">The full list of issues to export.</param>
        /// <param name="filePath">The full file system path for the output .xlsx file.</param>
        /// <returns>True if export succeeded; false otherwise.</returns>
        Task<bool> ExportContractorReportAsync(List<SnagIssue> issues, string filePath);
    }
}
