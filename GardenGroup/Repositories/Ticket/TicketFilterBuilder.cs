// Author: Mart
using GardenGroup.Enums;
using GardenGroup.Models;
using MongoDB.Driver;

namespace GardenGroup.Repositories;

//Builds the status / assignee / reporter filter in one place. TicketRepository, TicketPrioritySorter and TicketSearcher all use it, so the three list queries cannot filter differently.

internal static class TicketFilterBuilder
{
    //Null parameters are ignored. All parameters null gives an empty filter (all tickets).
    public static FilterDefinition<Ticket> Build(Statuses? status, int? assignedToId, int? reportedById)
    {
        var f = Builders<Ticket>.Filter;
        var filter = f.Empty;

        if (status.HasValue)
            filter &= f.Eq(t => t.Status, status.Value);

        if (assignedToId.HasValue)
            filter &= f.Eq(t => t.AssignedToId, assignedToId.Value);

        if (reportedById.HasValue)
            filter &= f.Eq(t => t.ReportById, reportedById.Value);

        return filter;
    }
}