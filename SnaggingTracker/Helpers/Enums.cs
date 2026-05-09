namespace SnaggingTracker.Helpers
{
    /// <summary>
    /// Represents the current workflow status of a snagging issue.
    /// </summary>
    public enum IssueStatus
    {
        Open,
        InProgress,
        Closed
    }

    /// <summary>
    /// Represents the urgency level of a snagging issue.
    /// </summary>
    public enum IssuePriority
    {
        Low,
        Medium,
        High
    }
}
