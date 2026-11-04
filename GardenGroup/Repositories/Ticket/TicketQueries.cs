// Author: Carlo
using GardenGroup.Enums;
using GardenGroup.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GardenGroup.Repositories;

/// <summary>MongoDB aggregation pipelines for the dashboard statistics.</summary>
public class TicketQueries : ITicketQueries
{
    private readonly IMongoCollection<Ticket> _tickets;

    public TicketQueries(IMongoDatabase db) => _tickets = db.GetCollection<Ticket>("tickets");

    /// <inheritdoc />
    public async Task<List<StatusPercentage>> GetStatusPercentagesAsync(int? reportedBy)
    {
        var pipeline = new List<BsonDocument>();

        // Filter first: smaller set for the $group, and an index on reportedBy can be used.
        if (reportedBy.HasValue)
            pipeline.Add(new BsonDocument("$match", new BsonDocument("reportedBy", reportedBy.Value)));

        // Status is compared in lower case, so "Open", "open" and "OPEN" in the database all count the same.
        static BsonDocument StatusIs(string lowerCaseValue) =>
            new BsonDocument("$eq", new BsonArray { new BsonDocument("$toLower", "$status"), lowerCaseValue });

        var hasResolvedAt = new BsonDocument("$ne", new BsonArray
        {
            new BsonDocument("$ifNull", new BsonArray { "$resolvedAt", BsonNull.Value }),
            BsonNull.Value
        });

        // Derive the dashboard category per ticket. The order of the branches matters:
        //  1. Open always wins (a reopened ticket may still have an old resolvedAt).
        //  2. Resolved = status Resolved, or resolvedAt filled (a ticket that was solved and then closed).
        //  3. Closed without a resolvedAt = closed without resolve.
        //  Default: everything else is not yet solved, so it counts as open.
        var category = new BsonDocument("$switch", new BsonDocument
        {
            { "branches", new BsonArray
                {
                    new BsonDocument { { "case", StatusIs("open") }, { "then", "Open" } },
                    new BsonDocument
                    {
                        { "case", new BsonDocument("$or", new BsonArray { StatusIs("resolved"), hasResolvedAt }) },
                        { "then", "Resolved" }
                    },
                    new BsonDocument { { "case", StatusIs("closed") }, { "then", "ClosedWithoutResolve" } }
                } },
            { "default", "Open" }
        });

        // Count per category.
        pipeline.Add(new BsonDocument("$group", new BsonDocument
        {
            { "_id", category },
            { "count", new BsonDocument("$sum", 1) }
        }));

        // Calculate the total and keep the rows.
        pipeline.Add(new BsonDocument("$group", new BsonDocument
        {
            { "_id", BsonNull.Value },
            { "total", new BsonDocument("$sum", "$count") },
            { "rows", new BsonDocument("$push", new BsonDocument { { "category", "$_id" }, { "count", "$count" } }) }
        }));

        // One document per category again, and calculate the percentage.
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

        // $group returns no row for a category without tickets: add it with 0 (no division by zero),
        // and return the categories in a fixed order.
        foreach (var c in Enum.GetValues<TicketCategory>())
            if (rows.All(r => r.Category != c))
                rows.Add(new StatusPercentage { Category = c });

        return rows.OrderBy(r => r.Category).ToList();
    }

    /// <inheritdoc />
    public Task<List<EmployeeWorkload>> GetEmployeeWorkloadsAsync() =>
        _tickets.Aggregate<EmployeeWorkload>(new[]
        {
            // Tickets without an assignee do not count.
            new BsonDocument("$match", new BsonDocument("assignedTo", new BsonDocument("$ne", BsonNull.Value))),
            // Group first, then $lookup: one lookup per employee instead of one per ticket.
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$assignedTo" },
                { "ticketCount", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "employees" }, { "localField", "_id" },
                { "foreignField", "employeeId" }, { "as", "employee" }
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
                        "Unknown"
                    }) }
            }),
            new BsonDocument("$sort", new BsonDocument("ticketCount", -1))
        }).ToListAsync();
}
