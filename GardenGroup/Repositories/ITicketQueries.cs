using GardenGroup.Models;

namespace GardenGroup.Repositories;

/// <summary>
/// Defines the aggregation queries that power the dashboard.
/// </summary>
public interface ITicketQueries
{
    /// <summary>
    /// Counts the tickets per status and calculates each status as a percentage of the total.
    /// The calculation runs inside MongoDB, so only the result rows are sent to the application.
    /// </summary>
    /// <param name="reportedBy">
    /// Optional employee id. When set, only tickets reported by this employee are counted.
    /// </param>
    /// <returns>
    /// One row per status, sorted as returned by MongoDB. Statuses without tickets are included
    /// with a count of 0, so an empty collection returns all statuses with 0.
    /// </returns>
    Task<List<StatusPercentage>> GetStatusPercentagesAsync(int? reportedBy);

    /// <summary>
    /// Counts the tickets per assigned employee. Tickets without a handler are not counted.
    /// </summary>
    /// <returns>
    /// One row per employee with at least one assigned ticket, sorted from highest to lowest
    /// ticket count. The employee name is "Unknown" when no matching employee exists.
    /// </returns>
    Task<List<EmployeeWorkload>> GetEmployeeWorkloadsAsync();
}