using GardenGroup.Enums;
using GardenGroup.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GardenGroup.Repositories;

/// <summary>
/// MongoDB aggregation pipelines for the dashboard statistics.
/// </summary>
public class TicketQueries : ITicketQueries
{
    private readonly IMongoCollection<Ticket> _tickets;

    public TicketQueries(IMongoDatabase db) => _tickets = db.GetCollection<Ticket>("tickets");
    /// <inheritdoc />
    public async Task<List<StatusPercentage>> GetStatusPercentagesAsync(int? reportedBy)
    {
        var pipeline = new List<BsonDocument>();

        // Filter eerst: kleinere set voor de $group, en de index kan gebruikt worden.
        if (reportedBy.HasValue)
            pipeline.Add(new BsonDocument("$match", new BsonDocument("reportedBy", reportedBy.Value)));

        pipeline.Add(new BsonDocument("$group", new BsonDocument {
            { "_id", "$status" }, { "count", new BsonDocument("$sum", 1) } }));

        // Totaal berekenen en de rijen bewaren.
        pipeline.Add(new BsonDocument("$group", new BsonDocument {
            { "_id", BsonNull.Value },
            { "total", new BsonDocument("$sum", "$count") },
            { "rows", new BsonDocument("$push", new BsonDocument { { "status", "$_id" }, { "count", "$count" } }) } }));

        pipeline.Add(new BsonDocument("$unwind", "$rows"));

        pipeline.Add(new BsonDocument("$project", new BsonDocument {
            { "_id", 0 },
            { "status", "$rows.status" },
            { "count", "$rows.count" },
            { "percentage", new BsonDocument("$round", new BsonArray {
                new BsonDocument("$multiply", new BsonArray {
                    new BsonDocument("$divide", new BsonArray { "$rows.count", "$total" }), 100 }), 1 }) } }));

        var rows = await _tickets.Aggregate<StatusPercentage>(pipeline).ToListAsync();

        // $group geeft geen rij voor een status zonder tickets: aanvullen met 0.
        //maak alles kleine letters want zo is het in de database en het is hoofdlettergevoelig
        static string Norm(string s) => s.Replace(" ", "").Replace("_", "").ToLowerInvariant();

        foreach (var name in Enum.GetNames<TicketStatus>())
            if (!rows.Any(r => Norm(r.Status) == Norm(name)))
                rows.Add(new StatusPercentage { Status = name });

        return rows;
    }
    /// <inheritdoc />
    public Task<List<EmployeeWorkload>> GetEmployeeWorkloadsAsync() =>
        _tickets.Aggregate<EmployeeWorkload>(new[]
        {
            // Tickets zonder behandelaar tellen niet mee.
            new BsonDocument("$match", new BsonDocument("assignedTo", new BsonDocument("$ne", BsonNull.Value))),
            // Eerst groeperen, dan pas $lookup: één lookup per medewerker, niet per ticket.
            new BsonDocument("$group", new BsonDocument {
                { "_id", "$assignedTo" }, { "ticketCount", new BsonDocument("$sum", 1) } }),
            new BsonDocument("$lookup", new BsonDocument {
                { "from", "employees" }, { "localField", "_id" }, { "foreignField", "employeeId" }, { "as", "employee" } }),
            new BsonDocument("$set", new BsonDocument("employee", new BsonDocument("$first", "$employee"))),
            new BsonDocument("$project", new BsonDocument {
                { "_id", 0 },
                { "employeeId", "$_id" },
                { "ticketCount", 1 },
                { "name", new BsonDocument("$ifNull", new BsonArray {
            new BsonDocument("$concat", new BsonArray { "$employee.firstName", " ", "$employee.lastName" }),
                "Unknown" }) } }),
            new BsonDocument("$sort", new BsonDocument("ticketCount", -1))
        }).ToListAsync();
}