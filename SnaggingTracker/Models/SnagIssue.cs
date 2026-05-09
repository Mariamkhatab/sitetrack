using CommunityToolkit.Mvvm.ComponentModel;
using SnaggingTracker.Helpers;
using System;

namespace SnaggingTracker.Models
{
    /// <summary>
    /// Represents a single snagging/defect issue on a construction site.
    /// This is the core domain model. It is observable so changes propagate
    /// through bindings without requiring manual PropertyChanged calls.
    /// </summary>
    public partial class SnagIssue : ObservableObject
    {
        // --- Identity ---

        /// <summary>
        /// Unique identifier for the issue. Generated once at creation time.
        /// </summary>
        [ObservableProperty]
        private Guid _id;

        /// <summary>
        /// Auto-incremented human-readable reference number (e.g., SNAG-001).
        /// Assigned by the DataService on creation.
        /// </summary>
        [ObservableProperty]
        private string _referenceNumber = string.Empty;

        // --- Core Details ---

        /// <summary>
        /// Short, descriptive title of the issue.
        /// </summary>
        [ObservableProperty]
        private string _title = string.Empty;

        /// <summary>
        /// Full description of the defect or required corrective action.
        /// </summary>
        [ObservableProperty]
        private string _description = string.Empty;

        /// <summary>
        /// Physical location on site (e.g., "Block A - Level 3", "Rooftop Plant Room").
        /// </summary>
        [ObservableProperty]
        private string _location = string.Empty;

        // --- Classification ---

        /// <summary>
        /// The urgency level of this issue.
        /// </summary>
        [ObservableProperty]
        private IssuePriority _priority = IssuePriority.Medium;

        /// <summary>
        /// The current workflow status of this issue.
        /// </summary>
        [ObservableProperty]
        private IssueStatus _status = IssueStatus.Open;

        // --- Assignment ---

        /// <summary>
        /// The party responsible for resolving this issue
        /// (e.g., "Subcontractor A", "MEP Team", "Main Contractor").
        /// </summary>
        [ObservableProperty]
        private string _responsibleParty = string.Empty;

        // --- Dates ---

        /// <summary>
        /// The date the issue was identified and logged.
        /// </summary>
        [ObservableProperty]
        private DateTime _dateLogged = DateTime.Today;

        /// <summary>
        /// The target date by which the issue must be resolved.
        /// Nullable — not all issues will have a defined due date at logging time.
        /// </summary>
        [ObservableProperty]
        private DateTime? _dueDate;

        /// <summary>
        /// The date the issue was marked as Closed.
        /// Null if the issue is still Open or In Progress.
        /// </summary>
        [ObservableProperty]
        private DateTime? _dateResolved;

        // --- Audit ---

        /// <summary>
        /// Free-text notes for corrective actions taken, inspector comments, etc.
        /// Appended over time as the issue progresses.
        /// </summary>
        [ObservableProperty]
        private string _remarks = string.Empty;
    }
}
