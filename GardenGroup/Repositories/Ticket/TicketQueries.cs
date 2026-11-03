using GardenGroup.Enums;
using GardenGroup.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GardenGroup.Repositories;

/// <inheritdoc />
public class TicketQueries : ITicketQueries
{
    private readonly IMongoCollection<Ticket> _tickets;

    public TicketQueries(IMongoDatabase db) => _tickets = db.GetCollection<Ticket>("tickets");

    /// <inheritdoc />
    public async Task<List<StatusPercentage>> GetStatusPercentagesAsync(int? reportedBy)
    {
        var pipeline = new List<BsonDocument>();

        // 1. Filter eerst: kleinere set voor de $group en een index op reportedBy kan gebruikt worden.
        if (reportedBy.HasValue)
            pipeline.Add(new BsonDocument("$match", new BsonDocument("reportedBy", reportedBy.Value)));

        // 2. Categorie afleiden per ticket. Volgorde van de branches is belangrijk:
        //    Open wint altijd (ook als een heropend ticket nog een oude resolvedAt heeft).
        //    Resolved = status Resolved OF resolvedAt gevuld (ook een gesloten, eerder opgelost ticket).
        //    Alles wat overblijft is gesloten zonder oplossing.
        var category = new BsonDocument("$switch", new BsonDocument
        {
            { "branches", new BsonArray
                {
                    new BsonDocument
                    {
                        { "case", new BsonDocument("$eq", new BsonArray { "$status", "Open" }) },
                        { "then", "Open" }
                    },
                    new BsonDocument
                    {
                        { "case", new BsonDocument("$or", new BsonArray
                            {
                                new BsonDocument("$eq", new BsonArray { "$status", "Resolved" }),
                                new BsonDocument("$ne", new BsonArray
                                {
                                    new BsonDocument("$ifNull", new BsonArray { "$resolvedAt", BsonNull.Value }),
                                    BsonNull.Value
                                })
                            }) },
                        { "then", "Resolved" }
                    }
                } },
            { "default", "ClosedWithoutResolve" }
        });

        // 3. Tellen per categorie.
        pipeline.Add(new BsonDocument("$group", new BsonDocument
        {
            { "_id", category },
            { "count", new BsonDocument("$sum", 1) }
        }));

        // 4. Totaal berekenen en de rijen bewaren.
        pipeline.Add(new BsonDocument("$group", new BsonDocument
        {
            { "_id", BsonNull.Value },
            { "total", new BsonDocument("$sum", "$count") },
            { "rows", new BsonDocument("$push", new BsonDocument { { "category", "$_id" }, { "count", "$count" } }) }
        }));

        // 5. Terug naar één document per categorie en het percentage uitrekenen.
        pipeline.Add(new BsonDocument("$unwind", "$rows"));
        pipeline.Add(new BsonDocument("$project", new BsonDocument
        {
            { "_id", 0 },
            { "category", "$rows.category" },
            { "count", "$rows.count" },
            { "percentage", new BsonDocument("$round", new BsonArray
                {
                    new BsonDocument("$multiply", new BsonArray
                    {
                        new BsonDocument("$divide", new BsonArray { "$rows.count", "$total" }), 100
                    }),
                    1
                }) }
        }));

        var rows = await _tickets.Aggregate<StatusPercentage>(pipeline).ToListAsync();

        // $group geeft geen rij voor een categorie zonder tickets: aanvullen met 0
        // en in vaste volgorde teruggeven (Open, Resolved, ClosedWithoutResolve).
        foreach (var c in Enum.GetValues<TicketCategory>())
            if (rows.All(r => r.Category != c))
                rows.Add(new StatusPercentage { Category = c });

        return rows.OrderBy(r => r.Category).ToList();
    }

    /// <inheritdoc />
    public Task<List<EmployeeWorkload>> GetEmployeeWorkloadsAsync() =>
        _tickets.Aggregate<EmployeeWorkload>(new[]
        {
            // Tickets zonder behandelaar tellen niet mee.
            new BsonDocument("$match", new BsonDocument("assignedTo", new BsonDocument("$ne", BsonNull.Value))),
            // Eerst groeperen, dan pas $lookup: één lookup per medewerker in plaats van één per ticket.
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$assignedTo" },
                { "ticketCount", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "employees" }, { "localField", "_id" },
                { "foreignField", "_id" }, { "as", "employee" }
            }),
            new BsonDocument("$set", new BsonDocument("employee", new BsonDocument("$first", "$employee"))),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { "employeeId", "$_id" },
                { "ticketCount", 1 },
                { "name", new BsonDocument("$ifNull", new BsonArray
                    {
                        new BsonDocument("$concat", new BsonArray { "$employee.firstName", " ", "$employee.lastName" }),
                        "Onbekend"
                    }) }
            }),
            new BsonDocument("$sort", new BsonDocument("ticketCount", -1))
        }).ToListAsync();
}
