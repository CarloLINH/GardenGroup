// Author: Mart
using GardenGroup.Enums;

namespace GardenGroup.Models;

//Everything the ticket list needs to know in one object instead of a growing parameter list.
public sealed class TicketListQuery
{
    public Statuses? Status { get; init; }
    public int? AssignedToId { get; init; }
    public int? ReportedById { get; init; }
    public string? Search { get; init; }
    public string Sort { get; init; } = TicketSort.Newest;
    public int Page { get; init; }
    public int PageSize { get; init; } = 100;
}