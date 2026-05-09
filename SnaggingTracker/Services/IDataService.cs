using SnaggingTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnaggingTracker.Services
{
    /// <summary>
    /// Defines the contract for loading and saving snagging issue data.
    /// The ViewModel depends on this abstraction, not a concrete implementation,
    /// keeping the architecture loosely coupled and testable.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Asynchronously loads all snagging issues from the data store.
        /// Returns an empty list (never null) if no data exists yet.
        /// </summary>
        Task<List<SnagIssue>> LoadIssuesAsync();

        /// <summary>
        /// Asynchronously persists the full list of snagging issues to the data store.
        /// Overwrites the previous state entirely (single source of truth).
        /// </summary>
        /// <param name="issues">The complete, current list of issues to save.</param>
        Task SaveIssuesAsync(List<SnagIssue> issues);

        /// <summary>
        /// Generates and assigns the next sequential reference number
        /// (e.g., "SNAG-001", "SNAG-042") to a newly created issue.
        /// </summary>
        /// <param name="existingIssues">The current list, used to determine the next index.</param>
        /// <returns>A formatted reference string.</returns>
        string GenerateReferenceNumber(List<SnagIssue> existingIssues);
    }
}
