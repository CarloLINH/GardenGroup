using GardenGroup.Models;
using MongoDB.Driver;

namespace GardenGroup.Repositories;

// Creates all MongoDB indexes once at application startup.
public static class MongoIndexInitializer
{
    public static void EnsureIndexes(IMongoDatabase db)
    {
        var tickets = db.GetCollection<Ticket>("tickets");
        var employees = db.GetCollection<Employee>("employees");

        CreateTicketIndexes(tickets);
        CreateEmployeeIndexes(employees);
    }

    private static void CreateTicketIndexes(
        IMongoCollection<Ticket> tickets)
    {
        var keys = Builders<Ticket>.IndexKeys;

        // Old single-field indexes are now a prefix of the compound indexes, so they only cost writes.
        foreach (var legacy in new[] { "status_1", "assignedTo_1" })
        {
            try { tickets.Indexes.DropOne(legacy); }
            catch (MongoCommandException ex) when (ex.CodeName is "IndexNotFound" or "NamespaceNotFound") { }
        }

        tickets.Indexes.CreateMany(new[]
        {
            //Filter by status
            new CreateIndexModel<Ticket>(
                keys.Ascending(t => t.Status).Descending(t => t.CreatedAt),
                new CreateIndexOptions { Name = "ix_status_createdAt" }),

            //Tickets of one employee, newest first
            new CreateIndexModel<Ticket>(
                keys.Ascending(t => t.AssignedToId).Descending(t => t.CreatedAt),
                new CreateIndexOptions { Name = "ix_assignedTo_createdAt" }),

            //Sort all tickets by date
            new CreateIndexModel<Ticket>(
                keys.Descending(t => t.CreatedAt),
                new CreateIndexOptions { Name = "createdAt_-1" }),

            //Status percentages per reporter
            new CreateIndexModel<Ticket>(
                keys.Ascending(t => t.ReportById).Ascending(t => t.Status),
                new CreateIndexOptions { Name = "ix_reportedBy_status" }),
        });

        CreateUniqueIndex(tickets, keys.Ascending(ticket => ticket.TicketId), "ux_ticketId");
    }

    private static void CreateEmployeeIndexes(
        IMongoCollection<Employee> employees)
    {
        var keys = Builders<Employee>.IndexKeys;

        CreateUniqueIndex( employees, keys.Ascending(employee => employee.EmployeeId), "ux_employeeId");

        CreateUniqueIndex( employees, keys.Ascending(employee => employee.Email), "ux_employee_email");
    }

    private static void CreateUniqueIndex<TDocument>(IMongoCollection<TDocument> collection, IndexKeysDefinition<TDocument> keys, string indexName)
    {
        try
        {
            collection.Indexes.CreateOne(new CreateIndexModel<TDocument>(
                    keys,
                    new CreateIndexOptions { Name = indexName, Unique = true }));
        }
        catch (MongoCommandException ex)
        {
            Console.WriteLine($"Unique index '{indexName}' was not created: {ex.Message}");
        }
    }
}