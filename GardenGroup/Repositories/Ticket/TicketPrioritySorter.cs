// Author: Carlo
using GardenGroup.Enums;
using GardenGroup.Models;
using MongoDB.Driver;

namespace GardenGroup.Repositories;

public class TicketPrioritySorter : ITicketPrioritySorter
{
    private const int MaxPageSize = 100;
    private readonly IMongoCollection<Ticket> _tickets;

    public TicketPrioritySorter(IMongoDatabase db) => _tickets = db.GetCollection<Ticket>("tickets");

    public async Task<List<Ticket>> GetSortedAsync(
 TicketSortOrder order, int? assignedTo, int? reportedBy, Statuses? status, int page, int pageSize)
    {
        if (!Enum.IsDefined(order)) throw new ArgumentOutOfRangeException(nameof(order));
        if (page < 0) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

        pageSize = Math.Min(pageSize, MaxPageSize);

        var filter = TicketFilterBuilder.Build(status, assignedTo, reportedBy);

        return await _tickets.Find(filter)
            .Project<Ticket>(Builders<Ticket>.Projection.Exclude(t => t.Description))
            .Sort(BuildSort(order))
            .Skip(page * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    // CreatedAt en Id houden de volgorde vast bij gelijke prioriteit, anders kan een ticket
    // bij paginering dubbel of nooit verschijnen. Laag naar hoog is de exacte omkering,
    // zodat dezelfde index beide richtingen bedient.
    // De indexen voor deze sortering staan in MongoIndexInitializer (ix_priority_sort e.d.). -Mart
    private static SortDefinition<Ticket> BuildSort(TicketSortOrder order)
    {
        var s = Builders<Ticket>.Sort;
        return order == TicketSortOrder.PriorityHighToLow
            ? s.Descending(t => t.Priority).Descending(t => t.CreatedAt).Descending(t => t.Id)
            : s.Ascending(t => t.Priority).Ascending(t => t.CreatedAt).Ascending(t => t.Id);
    }
}